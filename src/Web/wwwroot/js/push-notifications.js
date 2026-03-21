// OpenHR Push Notification Infrastructure
// Handles permission requests, subscription registration, and push event handling.
// Works with the /api/v1/notiser/push-subscription endpoint.

(function () {
    "use strict";

    window.OpenHR = window.OpenHR || {};

    window.OpenHR.push = {
        _vapidPublicKey: null,
        _registration: null,

        /**
         * Initialize push notifications.
         * @param {string} vapidPublicKey - The VAPID public key (base64url encoded)
         */
        init: async function (vapidPublicKey) {
            this._vapidPublicKey = vapidPublicKey;

            if (!("serviceWorker" in navigator) || !("PushManager" in window)) {
                console.warn("Push notifications are not supported in this browser.");
                return false;
            }

            try {
                this._registration = await navigator.serviceWorker.ready;
                console.log("Push notification infrastructure ready.");
                return true;
            } catch (err) {
                console.warn("Failed to get service worker registration:", err);
                return false;
            }
        },

        /**
         * Request notification permission from the user.
         * @returns {Promise<string>} Permission state: "granted", "denied", or "default"
         */
        requestPermission: async function () {
            if (!("Notification" in window)) {
                return "denied";
            }

            if (Notification.permission === "granted") {
                return "granted";
            }

            const result = await Notification.requestPermission();
            return result;
        },

        /**
         * Subscribe to push notifications and register with the server.
         * @param {string} employeeId - The employee's GUID
         * @returns {Promise<boolean>} Whether subscription succeeded
         */
        subscribe: async function (employeeId) {
            if (!this._registration || !this._vapidPublicKey) {
                console.warn("Push not initialized. Call init() first.");
                return false;
            }

            const permission = await this.requestPermission();
            if (permission !== "granted") {
                console.warn("Notification permission not granted.");
                return false;
            }

            try {
                // Check for existing subscription
                let subscription = await this._registration.pushManager.getSubscription();

                if (!subscription) {
                    // Create new subscription
                    const applicationServerKey = this._urlBase64ToUint8Array(this._vapidPublicKey);
                    subscription = await this._registration.pushManager.subscribe({
                        userVisibleOnly: true,
                        applicationServerKey: applicationServerKey
                    });
                }

                // Extract keys
                const rawKey = subscription.getKey("p256dh");
                const rawAuth = subscription.getKey("auth");
                const p256dh = rawKey ? btoa(String.fromCharCode.apply(null, new Uint8Array(rawKey))) : "";
                const auth = rawAuth ? btoa(String.fromCharCode.apply(null, new Uint8Array(rawAuth))) : "";

                // Register with server
                const response = await fetch("/api/v1/notiser/push-subscription", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({
                        anstallId: employeeId,
                        endpoint: subscription.endpoint,
                        p256dhKey: p256dh,
                        authKey: auth
                    })
                });

                if (response.ok) {
                    console.log("Push subscription registered successfully.");
                    return true;
                } else {
                    console.warn("Failed to register push subscription:", response.status);
                    return false;
                }
            } catch (err) {
                console.error("Push subscription failed:", err);
                return false;
            }
        },

        /**
         * Unsubscribe from push notifications.
         * @returns {Promise<boolean>} Whether unsubscription succeeded
         */
        unsubscribe: async function () {
            if (!this._registration) return false;

            try {
                const subscription = await this._registration.pushManager.getSubscription();
                if (subscription) {
                    await subscription.unsubscribe();
                    console.log("Push subscription removed.");
                    return true;
                }
                return false;
            } catch (err) {
                console.warn("Push unsubscribe failed:", err);
                return false;
            }
        },

        /**
         * Check if currently subscribed to push notifications.
         * @returns {Promise<boolean>}
         */
        isSubscribed: async function () {
            if (!this._registration) return false;

            try {
                const subscription = await this._registration.pushManager.getSubscription();
                return subscription !== null;
            } catch {
                return false;
            }
        },

        /**
         * Convert a base64url VAPID key to Uint8Array for subscription.
         * @param {string} base64String
         * @returns {Uint8Array}
         */
        _urlBase64ToUint8Array: function (base64String) {
            const padding = "=".repeat((4 - base64String.length % 4) % 4);
            const base64 = (base64String + padding).replace(/-/g, "+").replace(/_/g, "/");
            const rawData = window.atob(base64);
            const outputArray = new Uint8Array(rawData.length);
            for (var i = 0; i < rawData.length; ++i) {
                outputArray[i] = rawData.charCodeAt(i);
            }
            return outputArray;
        }
    };
})();
