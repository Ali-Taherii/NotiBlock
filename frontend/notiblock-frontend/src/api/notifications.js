/* eslint-disable no-unused-vars */
import apiClient from "./api";

export async function getNotifications() {
    return await apiClient.get("/notifications");
}

export async function getNotificationById(id) {
    return await apiClient.get(`/notifications/${id}`);
}

export async function deleteNotification(id) {
    return await apiClient.delete(`/notifications/${id}`);
}

export async function countUnreadNotifications() {
    return await apiClient.get("/notifications/unread-count");
}

// needs fix in the backend
export async function markAsRead(_notificationIds) {
    return await apiClient.post(`/notifications/mark-as-read`);
}

export async function markAllAsRead() {
    return await apiClient.post(`/notifications/mark-all-as-read`);
}