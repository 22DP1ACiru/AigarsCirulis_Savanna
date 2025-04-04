import * as Const from './game-constants.js';
import * as State from './game-state.js';
import * as UI from './game-ui.js';

const SIGNALR_HUB_URL = "/gameHub";
const LOG_LEVEL = signalR.LogLevel.Warning;

const connection = new signalR.HubConnectionBuilder()
    .withUrl(SIGNALR_HUB_URL)
    .configureLogging(LOG_LEVEL)
    .withAutomaticReconnect()
    .build();

let isFetchingStats = false; // Prevent multiple simultaneous fetches

// --- Hub Event Handlers ---
connection.on("ReceiveUpdate", (updatePayload) => {
    if (updatePayload && updatePayload.cells) {
        State.updateIteration(updatePayload.iterationCount);
        if (typeof updatePayload.livingAnimalCount !== 'undefined') {
            UI.updateAnimalCountDisplay(updatePayload.livingAnimalCount);
        }
        UI.updateIterationDisplay(); // Update iteration display
        UI.renderGrid(updatePayload.cells); // Update grid (handles highlight/stats display call)
        // Refresh stats if needed AFTER grid is rendered
        refreshSelectedStatsIfNeeded();
    } else { console.warn("Received invalid update payload."); }
});

connection.on("SaveStatus", (message, isSuccess) => {
    UI.showStatus(message, isSuccess);
    if (isSuccess) { State.setLastSave(); }
});

connection.on("GameStalled", (message) => {
    UI.showGridOverlayMessage(message);
    // Disable controls using UI function
    UI.disableGameControls(true);
});

connection.onclose(async (error) => {
    console.warn(`SignalR connection closed. Error: ${error}.`);
    UI.showStatus(Const.MSG_CONNECTION_LOST, false);
});

// --- Connection Management ---
export async function connect(sessionId) {
    if (connection.state === signalR.HubConnectionState.Connected) return true;
    try {
        await connection.start();
        console.log("SignalR Connected.");
        // Fetch display config
        try {
            const configMap = await connection.invoke("GetDisplayConfig");
            State.setAnimalIcons(configMap);
            console.log("Display config received:", State.getAnimalIcons());
        } catch (err) { console.error("Error fetching display config:", err); }
        // Join session
        await connection.invoke("JoinSession", sessionId);
        console.log(`Joined session group: ${sessionId}`);
        // Update initial UI state
        UI.updateDisplayModeButton();
        return true;
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
        UI.showStatus(Const.MSG_CONNECTION_ERROR, false);
        return false;
    }
}

// --- Hub Method Invocations ---
async function invokeHubMethod(methodName, ...args) {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        UI.showStatus(`Cannot ${methodName}: Not connected.`, false);
        return null; // Indicate failure
    }
    try {
        return await connection.invoke(methodName, ...args);
    } catch (err) {
        console.error(`Error invoking ${methodName}: ${err}`);
        UI.showStatus(`Error during ${methodName}: ${err.message || err}`, false);
        return null; // Indicate failure
    }
}

export async function invokeAddAnimal(sessionId, animalType) {
    return await invokeHubMethod("AddAnimal", sessionId, animalType);
}

export async function invokeResetGame(sessionId) {
    const result = await invokeHubMethod("ResetGame", sessionId);
    if (result !== null) State.resetLastSave(); // Optimistically reset save state
    return result;
}

export async function invokeSaveGame(sessionId, saveName) {
    const result = await invokeHubMethod("SaveGame", sessionId, saveName);
    // SaveStatus message handles state update
    if (result !== null) UI.clearSaveInput();
    return result;
}

export async function invokeGetStats(sessionId, animalId) {
    if (!animalId || isFetchingStats) return null; // Avoid fetching if no ID or already fetching
    isFetchingStats = true;
    try {
        const stats = await invokeHubMethod("GetAnimalStats", sessionId, animalId);
        if (stats && !stats.Error) {
            if (animalId === State.getSelectedAnimalId()) { // Check selection hasn't changed
                State.setSelectedAnimal(animalId, stats); // Update state with FRESH stats
                UI.updateStatsDisplay(); // Update UI
            }
        } else {
            // Animal not found or error from hub
            if (animalId === State.getSelectedAnimalId()) { // Clear only if it was still selected
                State.clearSelectedAnimal();
                UI.clearHighlight();
                UI.updateStatsDisplay(); // Clear UI display
            }
        }
        return stats; // Return raw stats or null/error
    } finally {
        isFetchingStats = false;
    }
}

// Function called by ReceiveUpdate to get fresh stats if needed
async function refreshSelectedStatsIfNeeded() {
    const selectedId = State.getSelectedAnimalId();
    if (selectedId) {
        await invokeGetStats(State.getSessionId(), selectedId); // State needs getSessionId()
    }
}
