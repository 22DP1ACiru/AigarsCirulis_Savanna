import * as Const from './game-constants.js';
import * as DOM from './game-dom.js'; // Import DOM elements/functions
import * as State from './game-state.js'; // Import state functions

let highlightedCellElement = null; // Keep track of highlighted DOM node

export function renderGrid(cells) {
    State.setCurrentGridCells(cells); // Update state

    if (!DOM.gameGridElement) return;

    if (State.getCurrentGridCells().length === 0) {
        DOM.gameGridElement.innerHTML = Const.MSG_WAITING_DATA;
        return;
    }

    let html = '';
    const grid = State.getCurrentGridCells();
    const useIcons = State.isUsingIcons();
    const animalIcons = State.getAnimalIcons();
    const emptyCellChar = DOM.gameGridElement.dataset.emptyChar; // Get from data attribute via DOM module

    grid.forEach(cell => {
        let displayContent = ' ';
        let animalIdAttr = '';
        if (cell.symbol && cell.symbol !== emptyCellChar && cell.animalId) {
            displayContent = (useIcons && cell.icon) ? cell.icon : cell.symbol;
            animalIdAttr = `data-animal-id="${cell.animalId}"`;
        }
        html += `<div class="game-cell" data-x="${cell.x}" data-y="${cell.y}" ${animalIdAttr}>${displayContent}</div>`;
    });
    DOM.gameGridElement.innerHTML = html;

    // Re-apply highlight
    reapplyHighlight();
}

function reapplyHighlight() {
    highlightedCellElement = null;
    const selectedId = State.getSelectedAnimalId();
    if (selectedId) {
        const selector = `.game-cell[data-animal-id="${selectedId}"]`;
        const newCell = DOM.gameGridElement.querySelector(selector);
        if (newCell) {
            newCell.classList.add('highlighted');
            highlightedCellElement = newCell;
        } else {
            State.clearSelectedAnimal(); // Animal disappeared
            updateStatsDisplay(); // Clear stats display
        }
    }
}

export function clearHighlight() {
    if (highlightedCellElement) {
        highlightedCellElement.classList.remove('highlighted');
        highlightedCellElement = null;
    }
}

export function applyHighlight(cellElement) {
    clearHighlight(); // Clear previous
    if (cellElement) {
        cellElement.classList.add('highlighted');
        highlightedCellElement = cellElement;
    }
}


export function updateStatsDisplay() {
    if (!DOM.animalStatsElement) return;
    const stats = State.getLastFetchedStats();
    const selectedId = State.getSelectedAnimalId();

    if (stats && selectedId) {
        DOM.animalStatsElement.innerHTML = `
             <strong>${stats.typeName || 'Unknown Type'}</strong><br>
             Age: ${stats.age ?? '-'} iterations<br>
             Health: ${stats.health?.toFixed(1) ?? '-'} <br>
             Offspring: ${stats.offspring ?? '-'} <br>
             Pos: (${stats.positionX ?? '?'}, ${stats.positionY ?? '?'})`;
        DOM.showElement(DOM.animalStatsElement);
    } else {
        DOM.animalStatsElement.innerHTML = 'Click an animal...';
    }
}

export function updateIterationDisplay() {
    if (DOM.iterationCountElement) {
        DOM.iterationCountElement.textContent = State.getCurrentIteration();
    }
}

export function updateAnimalCountDisplay(count) {
    if (DOM.animalCountElement) {
        DOM.animalCountElement.textContent = (typeof count === 'number') ? count : Const.DEFAULT_COUNT_DISPLAY;
    }
}

export function updateDisplayModeButton() {
    if (DOM.btnDisplayMode) {
        DOM.btnDisplayMode.textContent = State.isUsingIcons() ? Const.MSG_SWAP_TO_LETTER_DISPLAY : Const.MSG_SWAP_TO_ICON_DISPLAY;
    }
}

let statusTimeoutId = null;
export function showStatus(message, isSuccess) {
    if (!DOM.saveStatusElement) return;
    clearTimeout(statusTimeoutId); // Clear previous timeout
    DOM.saveStatusElement.textContent = message;
    DOM.saveStatusElement.className = isSuccess ? Const.CSS_CLASS_SAVE_SUCCESS : Const.CSS_CLASS_SAVE_ERROR;
    statusTimeoutId = setTimeout(() => {
        DOM.saveStatusElement.textContent = '';
        DOM.saveStatusElement.className = Const.CSS_CLASS_SAVE_DEFAULT;
    }, Const.CLEAR_STATUS_TIMEOUT_MS);
}

export function clearSaveInput() {
    DOM.clearValue(DOM.saveNameInput);
}
export function focusSaveInput() {
    if (DOM.saveNameInput) DOM.saveNameInput.focus();
}

export function disableGameControls(disabled) {
    if (DOM.btnAddAntelope) DOM.btnAddAntelope.disabled = disabled;
    if (DOM.btnAddLion) DOM.btnAddLion.disabled = disabled;
    if (DOM.btnSaveGame) DOM.btnSaveGame.disabled = disabled;
}

export function showGridOverlayMessage(message) {
    if (DOM.gridOverlayMessageElement && DOM.gridOverlayTextElement) {
        DOM.gridOverlayTextElement.textContent = message;
        DOM.gridOverlayMessageElement.style.display = 'flex'; // Use flex to show
    }
}

export function hideGridOverlayMessage() {
    if (DOM.gridOverlayMessageElement) {
        DOM.gridOverlayMessageElement.style.display = 'none'; // Hide it
    }
}

export function resetUIState() {
    State.resetLastSave(); // Reset unsaved changes counter in state
    State.clearSelectedAnimal(); // Clear selection state
    updateStatsDisplay(); // Clear stats display on UI
    clearHighlight(); // Clear highlight visually
    disableGameControls(false); // Re-enable Add/Save buttons
    hideGridOverlayMessage();
    // Clear any persistent stall message
    if (DOM.saveStatusElement && DOM.saveStatusElement.textContent.includes("stalled")) {
        showStatus("Game Reset!", true); // Show temporary success
    }
    console.log("UI state reset.");
}