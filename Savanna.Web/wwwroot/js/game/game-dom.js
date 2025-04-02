import * as Const from './game-constants.js';

export const gameGridElement = document.getElementById(Const.DOM_ID_GAME_GRID);
export const saveStatusElement = document.getElementById(Const.DOM_ID_SAVE_STATUS);
export const saveNameInput = document.getElementById(Const.DOM_ID_SAVE_NAME_INPUT);
export const iterationCountElement = document.getElementById(Const.DOM_ID_ITERATION_COUNT);
export const animalCountElement = document.getElementById(Const.DOM_ID_ANIMAL_COUNT);
export const btnDisplayMode = document.getElementById(Const.DOM_ID_DISPLAY_MODE_BUTTON);
export const animalStatsElement = document.getElementById(Const.DOM_ID_STATS_DISPLAY);

export const btnAddAntelope = document.getElementById('btnAddAntelope');
export const btnAddLion = document.getElementById('btnAddLion');
export const btnResetGame = document.getElementById('btnResetGame');
export const btnSaveGame = document.getElementById('btnSaveGame');

export function getElementById(id) {
    return document.getElementById(id);
}

export function showElement(element) {
    if (element) element.style.display = 'block';
}

export function hideElement(element) {
    if (element) element.style.display = 'none';
}

export function clearValue(inputElement) {
    if (inputElement) inputElement.value = '';
}