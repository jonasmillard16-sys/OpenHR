// SignalR notification listener (loaded by App.razor)
// Connects to the NotificationHub and shows browser notifications when messages arrive.
// In production, this would be initialized with a user-specific channel.

(function () {
    "use strict";

    window.OpenHR = window.OpenHR || {};

    window.OpenHR.notifications = {
        connection: null,

        init: function (userId) {
            if (!window.signalR) {
                console.warn("SignalR library not loaded — skipping notification setup.");
                return;
            }

            this.connection = new signalR.HubConnectionBuilder()
                .withUrl("/hubs/notifications")
                .withAutomaticReconnect()
                .build();

            this.connection.on("ReceiveNotification", function (title, message) {
                // Show browser notification if permitted
                if (Notification.permission === "granted") {
                    new Notification(title, { body: message, icon: "/favicon.ico" });
                }
            });

            this.connection.start()
                .then(function () {
                    if (userId) {
                        window.OpenHR.notifications.connection.invoke("JoinUserChannel", userId);
                    }
                    console.log("SignalR notifications connected.");
                })
                .catch(function (err) {
                    console.warn("SignalR connection failed:", err);
                });
        },

        requestPermission: function () {
            if ("Notification" in window && Notification.permission === "default") {
                Notification.requestPermission();
            }
        }
    };
})();
