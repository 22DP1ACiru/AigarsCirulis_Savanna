import * as Const from './game-constants.js';

export const gameGridElement = document.getElementById(Const.DOM_ID_GAME_GRID);
export const saveStatusElement = document.getElementById(Const.DOM_ID_SAVE_STATUS);
export const saveNameInput = document.getElementById(Const.DOM_ID_SAVE_NAME_INPUT);
export const iterationCountElement = document.getElementById(Const.DOM_ID_ITERATION_COUNT);
export const animalCountElement = document.getElementById(Const.DOM_ID_ANIMAL_COUNT);
export const animalStatsElement = document.getElementById(Const.DOM_ID_STATS_DISPLAY);
export const gridOverlayMessageElement = document.getElementById(Const.DOM_ID_GRID_OVERLAY);
export const gridOverlayTextElement = document.getElementById(Const.DOM_ID_GRID_OVERLAY_TEXT);

export const btnDisplayMode = document.getElementById(Const.DOM_ID_DISPLAY_MODE_BUTTON);
export const btnAddAntelope = document.getElementById(Const.DOM_ID_ADD_ANTELOPE_BUTTON);
export const btnAddLion = document.getElementById(Const.DOM_ID_ADD_LION_BUTTON);
export const btnResetGame = document.getElementById(Const.DOM_ID_RESET_GAME_BUTTON);
export const btnSaveGame = document.getElementById(Const.DOM_ID_SAVE_GAME_BUTTON);

export const pluginControlsContainer = document.getElementById(Const.DOM_ID_PLUGIN_CONTROLS);
export const pluginSelectElement = document.getElementById(Const.DOM_ID_PLUGIN_SELECT);
export const btnAddSelectedPluginAnimal = document.getElementById(Const.DOM_ID_PLUGIN_ADD_BUTTON);

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