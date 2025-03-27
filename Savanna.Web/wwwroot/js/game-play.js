import * as Const from './game-constants.js';

const SIGNALR_HUB_URL = "/gameHub";

// --- SignalR Connection Setup ---
const connection = new signalR.HubConnectionBuilder()
    .withUrl(SIGNALR_HUB_URL)
    .configureLogging(signalR.LogLevel.Warning)
    .withAutomaticReconnect()
    .build();

// --- DOM Elements ---
const gameGridElement = document.getElementById(Const.DOM_ID_GAME_GRID);
const saveStatusElement = document.getElementById(Const.DOM_ID_SAVE_STATUS);
const saveNameInput = document.getElementById(Const.DOM_ID_SAVE_NAME_INPUT);
const iterationCountElement = document.getElementById(Const.DOM_ID_ITERATION_COUNT);
const animalCountElement = document.getElementById(Const.DOM_ID_ANIMAL_COUNT);

// --- Get Razor values from data attributes ---
const sessionId = gameGridElement.dataset.sessionId;
const emptyCellChar = gameGridElement.dataset.emptyChar;

// --- State Variables for Unsaved Changes ---
let currentIteration = 0;
let lastSavedIteration = -1;
let isInitialLoad = true;

// --- Functions ---

// Update the visual game grid based on data from server
function updateGridDisplay(grid) {
    if (!gameGridElement) return;
    if (!grid || grid.length === 0 || grid[0].length === 0) {
        gameGridElement.innerHTML = Const.MSG_WAITING_DATA;
        return;
    }

    let html = '';
    const height = grid.length;    // Number of rows (y)
    const width = grid[0].length; // Number of columns (x)

    for (let y = 0; y < height; y++) {
        html += '<div class="game-row">';
        const row = grid[y];
        for (let x = 0; x < width; x++) {
            const cellContent = (row[x] && row[x] !== emptyCellChar) ? row[x] : ' '; // Use nbsp for spacing
            html += `<div class="game-cell">${cellContent}</div>`;
        }
        html += '</div>';
    }
    gameGridElement.innerHTML = html;
}

// Display status messages related to saving
function showSaveStatus(message, isSuccess) {
    if (!saveStatusElement) return;
    saveStatusElement.textContent = message;
    saveStatusElement.className = isSuccess ? Const.CSS_CLASS_SAVE_SUCCESS : Const.CSS_CLASS_SAVE_ERROR;
    // Clear status after a few seconds
    setTimeout(() => {
        if (saveStatusElement.textContent === message) { // Avoid clearing newer messages
            saveStatusElement.textContent = '';
            saveStatusElement.className = Const.CSS_CLASS_SAVE_DEFAULT;
        }
    }, Const.CLEAR_STATUS_TIMEOUT_MS);
}

// Handle SignalR connection errors
function handleConnectionError(err) {
    console.error("SignalR Connection Error: ", err);
    showSaveStatus(Const.MSG_CONNECTION_ERROR, false); // Inform user
}

// Update iteration count display and state variable
function updateIterationCount(count) {
    currentIteration = (typeof count === 'number') ? count : currentIteration; // Update state variable
    if (iterationCountElement) {
        iterationCountElement.textContent = currentIteration;
    } else {
        console.warn(`Iteration count element not found (#${Const.DOM_ID_ITERATION_COUNT})`);   
    }

    // If it's the very first update after loading the page,
    // set lastSavedIteration to the initial iteration count.
    if (isInitialLoad) {
        lastSavedIteration = currentIteration;
        isInitialLoad = false; // Only do this once
        console.log(`Initial load complete. Starting iteration: ${currentIteration}, Last saved: ${lastSavedIteration}`);
    }
}

// Update animal count display
function updateAnimalCount(count) {
    if (animalCountElement) {
        animalCountElement.textContent = (typeof count === 'number') ? count : Const.DEFAULT_COUNT_DISPLAY;
    } else {
        console.warn(`Animal count element not found (#${Const.DOM_ID_ANIMAL_COUNT})`);
    }
}

// --- SignalR Event Handlers ---

connection.on("ReceiveUpdate", (updatePayload) => { // Parameter is the payload object
    if (updatePayload) {
        // Extract data from the payload
        updateGridDisplay(updatePayload.grid); // Pass grid to grid display function
        updateIterationCount(updatePayload.iterationCount); // Pass count to iteration display function
        updateAnimalCount(updatePayload.livingAnimalCount); // Pass count to animal count display function
    } else {
        console.warn("Received empty or null update payload.");
    }
});

connection.on("SaveStatus", (message, isSuccess) => {
    showSaveStatus(message, isSuccess);
    if (isSuccess) {
        lastSavedIteration = currentIteration;
        console.log(`Save successful. Last saved iteration updated to: ${lastSavedIteration}`);
    }
});

// --- SignalR Connection Logic ---

async function startSignalR() {
    try {
        await connection.start();
        console.log("SignalR Connected.");
        // Join the specific session group ONCE connected
        await connection.invoke("JoinSession", sessionId);
        console.log(`Joined session group: ${sessionId}`);
    } catch (err) {
        handleConnectionError(err);
    }
}

// Handle connection closure (e.g., server restart, network issue)
connection.onclose(async (error) => {
    console.warn(`SignalR connection closed. Error: ${error}. Attempting to reconnect...`);
    // withAutomaticReconnect handles this, but you might add custom logic here
    showSaveStatus(Const.MSG_CONNECTION_LOST    , false);
});

// --- Game Action Functions (called by buttons) ---

async function addAnimal(animalType) {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        showSaveStatus(Const.MSG_CANNOT_ADD_ANIMAL, false); return;
    }
    try {
        await connection.invoke("AddAnimal", sessionId, animalType);
    } catch (err) {
        console.error(`Error invoking AddAnimal: ${err}`);
        showSaveStatus(Const.MSG_ADD_ANIMAL_ERROR + (err.message || err), false);
    }
}

async function resetGame() {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        showSaveStatus(Const.MSG_CANNOT_RESET, false); return;
    }

    if (confirm(Const.MSG_RESET_CONFIRM)) {
        try {
            await connection.invoke("ResetGame", sessionId);
            lastSavedIteration = 0; // Reset to 0 since game state starts anew
            isInitialLoad = false; // Ensure initial load logic doesn't re-trigger incorrectly
            console.log("Reset command sent. Last saved iteration reset to 0.");
        } catch (err) {
            console.error(`Error invoking ResetGame: ${err}`);
            showSaveStatus(Const.MSG_RESET_ERROR + (err.message || err), false);
        }
    }
}

async function saveGame() {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        showSaveStatus(Const.MSG_CANNOT_SAVE, false); return;
    }

    const saveName = saveNameInput.value.trim();
    if (!saveName) {
        showSaveStatus(Const.MSG_SAVE_NAME_EMPTY, false);
        saveNameInput.focus();
        return;
    }

    showSaveStatus(Const.MSG_SAVING, true); // Indicate saving is in progress
    try {
        // Invoke hub method, hub will send back "SaveStatus" message
        await connection.invoke("SaveGame", sessionId, saveName);
        saveNameInput.value = ''; // Clear input on successful initiation
    } catch (err) {
        console.error(`Error invoking SaveGame: ${err}`);
        showSaveStatus(Const.MSG_SAVE_INIT_ERROR + (err.message || err), false);
    }
}

// --- Initialization ---

document.addEventListener('DOMContentLoaded', (event) => {
    // Get button elements
    const btnAddAntelope = document.getElementById('btnAddAntelope');
    const btnAddLion = document.getElementById('btnAddLion');
    const btnResetGame = document.getElementById('btnResetGame');
    const btnSaveGame = document.getElementById('btnSaveGame');

    // Check if grid element exists before proceeding
    if (!gameGridElement || !sessionId) {
        console.error(Const.MSG_GRID_OR_SESSION_NOT_FOUND_ERROR);
        return; // Stop initialization
    }

    // Attach event listeners
    if (btnAddAntelope) {
        btnAddAntelope.addEventListener('click', () => addAnimal('Antelope')); // Pass type
    }
    if (btnAddLion) {
        btnAddLion.addEventListener('click', () => addAnimal('Lion')); // Pass type
    }
    if (btnResetGame) {
        btnResetGame.addEventListener('click', resetGame);
    }
    if (btnSaveGame) {
        btnSaveGame.addEventListener('click', saveGame);
    }

    startSignalR(); // Start the connection when the page loads
});

// --- Unsaved Changes Warning ---

window.addEventListener('beforeunload', function (e) {
    // Check if more than 20 iterations have passed since the last save
    // Only warn if lastSavedIteration isn't -1 (meaning initial state is known)
    const iterationsSinceSave = (lastSavedIteration === -1) ? 0 : (currentIteration - lastSavedIteration);

    if (iterationsSinceSave >= Const.ITERATION_THRESHOLD_FOR_WARNING) {
        const confirmationMessage = Const.UNSAVED_WARNING_MSG; 
        e.preventDefault();
        e.returnValue = confirmationMessage;
        return confirmationMessage; // For modern browsers
    }
    // If condition not met, do nothing - allow navigation silently
});