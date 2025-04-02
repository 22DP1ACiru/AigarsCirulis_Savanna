import * as Const from './game-constants.js';

let currentIteration = 0;
let lastSavedIteration = -1;
let isInitialLoad = true;
let useIcons = false;
let animalIcons = {};
let currentGridCells = [];
let selectedAnimalId = null;
let lastFetchedStats = null;
let currentSessionId = null;

export function setSessionId(id) {
    currentSessionId = id;
}

export function getSessionId() {
    return currentSessionId;
}

export function updateIteration(count) {
    currentIteration = (typeof count === 'number') ? count : currentIteration;
    if (isInitialLoad) {
        lastSavedIteration = currentIteration;
        isInitialLoad = false;
        console.log(`Initial load complete. Iteration: ${currentIteration}, Last saved: ${lastSavedIteration}`);
    }
    return currentIteration;
}
export function getCurrentIteration() { return currentIteration; }

export function setLastSave() {
    lastSavedIteration = currentIteration;
    console.log(`Save successful. Last saved iteration updated to: ${lastSavedIteration}`);
}
export function resetLastSave() {
    lastSavedIteration = 0;
    isInitialLoad = false; // Treat reset as initial load done
    console.log("Reset command sent. Last saved iteration reset to 0.");
}
export function shouldWarnBeforeUnload() {
    const iterationsSinceSave = (lastSavedIteration === -1) ? 0 : (currentIteration - lastSavedIteration);
    console.log(`Checking beforeunload: Current: ${currentIteration}, LastSaved: ${lastSavedIteration}, Diff: ${iterationsSinceSave}`);
    return iterationsSinceSave >= Const.ITERATION_THRESHOLD_FOR_WARNING;
}

export function isUsingIcons() { return useIcons; }
export function toggleUseIcons() { useIcons = !useIcons; return useIcons; }

export function setAnimalIcons(map) { animalIcons = map || {}; }
export function getAnimalIcons() { return animalIcons; }

export function setCurrentGridCells(cells) { currentGridCells = cells || []; }
export function getCurrentGridCells() { return currentGridCells; }

export function setSelectedAnimal(id, stats) {
    selectedAnimalId = id;
    lastFetchedStats = stats ? { ...stats, startIteration: currentIteration } : null;
}
export function getSelectedAnimalId() { return selectedAnimalId; }
export function getLastFetchedStats() { return lastFetchedStats; }
export function clearSelectedAnimal() {
    selectedAnimalId = null;
    lastFetchedStats = null;
}