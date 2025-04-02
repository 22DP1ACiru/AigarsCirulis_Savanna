// wwwroot/js/game-play.js (NEW FILE - Main Entry Point)
import * as Const from './game-constants.js';
import * as DOM from './game-dom.js';
import * as State from './game-state.js';
import * as UI from './game-ui.js';
import * as SignalR from './signalr-connection.js';

/** --- Initialization --- */
document.addEventListener('DOMContentLoaded', async () => {
    // Basic check for essential elements
    if (!DOM.gameGridElement || !DOM.gameGridElement.dataset.sessionId) {
        console.error(Const.MSG_GRID_OR_SESSION_NOT_FOUND_ERROR);
        alert(Const.MSG_GRID_OR_SESSION_NOT_FOUND_ERROR); // Inform user
        return; // Stop initialization
    }

    const sessionId = DOM.gameGridElement.dataset.sessionId;
    State.setSessionId(sessionId); // Store session ID in state module

    // Connect to SignalR and fetch initial config/state
    const connected = await SignalR.connect(sessionId);
    if (!connected) {
        UI.showStatus("Failed to connect to game server.", false);
        // Maybe disable controls?
        return;
    }

    // --- Attach Event Listeners ---

    // Add Animal Buttons
    if (DOM.btnAddAntelope) DOM.btnAddAntelope.addEventListener('click', () => SignalR.invokeAddAnimal(sessionId, 'Antelope'));
    if (DOM.btnAddLion) DOM.btnAddLion.addEventListener('click', () => SignalR.invokeAddAnimal(sessionId, 'Lion'));
    // Add listeners for plugin buttons here if/when implemented

    // Reset Button
    if (DOM.btnResetGame) DOM.btnResetGame.addEventListener('click', async () => {
        if (confirm(Const.MSG_RESET_CONFIRM)) {
            await SignalR.invokeResetGame(sessionId);
            // Note: State update happens via ReceiveUpdate after reset
        }
    });

    // Save Button
    if (DOM.btnSaveGame) DOM.btnSaveGame.addEventListener('click', async () => {
        const saveName = DOM.saveNameInput ? DOM.saveNameInput.value.trim() : '';
        if (!saveName) {
            UI.showStatus(Const.MSG_SAVE_NAME_EMPTY, false);
            UI.focusSaveInput();
            return;
        }
        UI.showStatus(Const.MSG_SAVING, true);
        await SignalR.invokeSaveGame(sessionId, saveName);
        // Note: Actual state update happens via SaveStatus message
    });

    // Display Mode Button
    if (DOM.btnDisplayMode) {
        DOM.btnDisplayMode.addEventListener('click', () => {
            State.toggleUseIcons();
            UI.updateDisplayModeButton();
            // Re-render immediately using current data
            UI.renderGrid(State.getCurrentGridCells());
        });
    }

    // Grid Click Listener (for selecting animals)
    DOM.gameGridElement.addEventListener('click', async (e) => {
        const cellElement = e.target.closest('.game-cell');
        if (cellElement) {
            const animalId = cellElement.dataset.animalId;
            UI.clearHighlight(); // Clear visual highlight immediately

            if (animalId) {
                // If clicking same animal, deselect it (optional UX)
                if (animalId === State.getSelectedAnimalId()) {
                    State.clearSelectedAnimal();
                    UI.updateStatsDisplay();
                    return; // Don't fetch stats again
                }

                // Select new animal - fetch its stats
                State.setSelectedAnimal(animalId, null); // Set ID, clear old stats
                UI.applyHighlight(cellElement);
                UI.updateStatsDisplay(); // Clear display
                await SignalR.invokeGetStats(sessionId, animalId); // Fetch and update
            } else {
                // Clicked empty cell
                State.clearSelectedAnimal();
                UI.updateStatsDisplay(); // Clear display
            }
        } else {
            // Clicked grid background/gap
            State.clearSelectedAnimal();
            UI.clearHighlight();
            UI.updateStatsDisplay();
        }
    });

    console.log("Game play initialization complete.");
});

// --- Unsaved Changes Warning ---
window.addEventListener('beforeunload', (e) => {
    if (State.shouldWarnBeforeUnload()) {
        const confirmationMessage = Const.UNSAVED_WARNING_MSG;
        e.preventDefault();
        e.returnValue = confirmationMessage;
        return confirmationMessage;
    }
});