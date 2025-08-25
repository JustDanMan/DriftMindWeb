// Timezone detection for DriftMindWeb
window.timezoneHelper = {
    // Get user's timezone identifier (e.g., "Europe/Berlin", "America/New_York")
    getUserTimeZone: function() {
        try {
            return Intl.DateTimeFormat().resolvedOptions().timeZone;
        } catch (error) {
            console.warn('Could not detect timezone:', error);
            return 'UTC'; // Fallback to UTC
        }
    },

    // Get timezone offset in minutes (for debugging/fallback)
    getTimeZoneOffset: function() {
        try {
            return new Date().getTimezoneOffset();
        } catch (error) {
            console.warn('Could not get timezone offset:', error);
            return 0; // Fallback to UTC
        }
    }
};
