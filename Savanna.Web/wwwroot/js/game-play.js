// --- SignalR Connection Setup ---
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/gameHub") // Matches MapHub configuration
    .configureLogging(signalR.LogLevel.Warning)
    .withAutomaticReconnect()
    .build();

// --- DOM Elements ---
const gameGridElement = document.getElementById('gameGrid');
const saveStatusElement = document.getElementById('saveStatus');
const saveNameInput = document.getElementById('saveGameName');
const iterationCountElement = document.getElementById('iterationCount');
const animalCountElement = document.getElementById('animalCount');

// --- Get Razor values from data attributes ---
const sessionId = gameGridElement.dataset.sessionId; // Read data-session-id
const emptyCellChar = gameGridElement.dataset.emptyChar; // Read data-empty-char

// --- State Variables for Unsaved Changes ---
let currentIteration = 0;
let lastSavedIteration = -1;
let isInitialLoad = true;

// --- Functions ---

// Update the visual game grid based on data from server
function updateGridDisplay(grid) {
    if (!gameGridElement) return;
    if (!grid || grid.length === 0 || grid[0].length === 0) {
        gameGridElement.innerHTML = '<p>Waiting for game data...</p>';
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
    saveStatusElement.className = isSuccess ? 'form-text text-success' : 'form-text text-danger';
    // Clear status after a few seconds
    setTimeout(() => {
        if (saveStatusElement.textContent === message) { // Avoid clearing newer messages
            saveStatusElement.textContent = '';
            saveStatusElement.className = 'form-text';
        }
    }, 5000);
}

// Handle SignalR connection errors
function handleConnectionError(err) {
    console.error("SignalR Connection Error: ", err);
    showSaveStatus("Connection error. Please refresh.", false); // Inform user
}

// Update iteration count display and state variable
function updateIterationCount(count) {
    currentIteration = (typeof count === 'number') ? count : currentIteration; // Update state variable
    if (iterationCountElement) {
        iterationCountElement.textContent = currentIteration;
    } else {
        console.warn("Iteration count element not found (#iterationCount)");
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
        animalCountElement.textContent = (typeof count === 'number') ? count : '-';
    } else {
        console.warn("Animal count element not found (#animalCount)");
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
    showSaveStatus("Connection lost. Attempting to reconnect...", false);
});

// --- Game Action Functions (called by buttons) ---

async function addAnimal(animalType) {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        showSaveStatus("Cannot add animal: Not connected.", false); return;
    }
    try {
        await connection.invoke("AddAnimal", sessionId, animalType);
    } catch (err) {
        console.error(`Error invoking AddAnimal: ${err}`);
        showSaveStatus(`Error adding animal: ${err.message || err}`, false);
    }
}

async function resetGame() {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        showSaveStatus("Cannot reset game: Not connected.", false); return;
    }
    if (confirm('Are you sure you want to reset the game state? This cannot be undone.')) {
        try {
            await connection.invoke("ResetGame", sessionId);
            lastSavedIteration = 0; // Reset to 0 since game state starts anew
            isInitialLoad = false; // Ensure initial load logic doesn't re-trigger incorrectly
            console.log("Reset command sent. Last saved iteration reset to 0.");
        } catch (err) {
            console.error(`Error invoking ResetGame: ${err}`);
            showSaveStatus(`Error resetting game: ${err.message || err}`, false);
        }
    }
}

async function saveGame() {
    if (connection.state !== signalR.HubConnectionState.Connected) {
        showSaveStatus("Cannot save game: Not connected.", false); return;
    }
    const saveName = saveNameInput.value.trim();
    if (!saveName) {
        showSaveStatus("Please enter a name for the save.", false);
        saveNameInput.focus();
        return;
    }

    showSaveStatus("Saving...", true); // Indicate saving is in progress
    try {
        // Invoke hub method, hub will send back "SaveStatus" message
        await connection.invoke("SaveGame", sessionId, saveName);
        saveNameInput.value = ''; // Clear input on successful initiation
    } catch (err) {
        console.error(`Error invoking SaveGame: ${err}`);
        showSaveStatus(`Error initiating save: ${err.message || err}`, false);
    }
}

// --- Initialization ---

startSignalR(); // Start the connection when the page loads

// --- Unsaved Changes Warning ---

window.addEventListener('beforeunload', function (e) {
    // Check if more than 20 iterations have passed since the last save
    // Only warn if lastSavedIteration isn't -1 (meaning initial state is known)
    const iterationsSinceSave = (lastSavedIteration === -1) ? 0 : (currentIteration - lastSavedIteration);

    if (iterationsSinceSave >= 20) {
        const confirmationMessage = 'You have unsaved progress. Are you sure you want to leave?';
        e.preventDefault();
        e.returnValue = confirmationMessage;
        return confirmationMessage; // For modern browsers
    }
    // If condition not met, do nothing - allow navigation silently
});