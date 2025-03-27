export const MSG_WAITING_DATA = '<p>Waiting for game data...</p>';
export const MSG_CONNECTION_ERROR = "Connection error. Please refresh.";
export const MSG_CONNECTION_LOST = "Connection lost. Attempting to reconnect...";
export const MSG_CANNOT_ADD_ANIMAL = "Cannot add animal: Not connected.";
export const MSG_ADD_ANIMAL_ERROR = "Error adding animal: ";
export const MSG_CANNOT_RESET = "Cannot reset game: Not connected.";
export const MSG_RESET_CONFIRM = 'Are you sure you want to reset the game state? This cannot be undone.';
export const MSG_RESET_ERROR = "Error resetting game: ";
export const MSG_CANNOT_SAVE = "Cannot save game: Not connected.";
export const MSG_SAVE_NAME_EMPTY = "Please enter a name for the save.";
export const MSG_SAVING = "Saving...";
export const MSG_SAVE_INIT_ERROR = "Error initiating save: ";
export const MSG_GRID_OR_SESSION_NOT_FOUND_ERROR = "Cannot initialize page: Game grid element or session ID not found.";

export const DOM_ID_GAME_GRID = 'gameGrid';
export const DOM_ID_SAVE_STATUS = 'saveStatus';
export const DOM_ID_SAVE_NAME_INPUT = 'saveGameName';
export const DOM_ID_ITERATION_COUNT = 'iterationCount';
export const DOM_ID_ANIMAL_COUNT = 'animalCount';

export const DEFAULT_COUNT_DISPLAY = '-';
export const ITERATION_THRESHOLD_FOR_WARNING = 20;
export const UNSAVED_WARNING_MSG = 'You have unsaved progress. Are you sure you want to leave?';
export const CLEAR_STATUS_TIMEOUT_MS = 5000;

export const CSS_CLASS_SAVE_SUCCESS = 'form-text text-success';
export const CSS_CLASS_SAVE_ERROR = 'form-text text-danger';
export const CSS_CLASS_SAVE_DEFAULT = 'form-text';