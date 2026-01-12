import apiClient from "./api";

export async function createTicket(data) {
    return await apiClient.post("/reseller-tickets", data);
}

export async function linkReportToTicket(ticketId, reportIds) {
    return await apiClient.post(`/reseller-tickets/${ticketId}/link-reports`, { reportIds });
}

export async function getTicketById(ticketId) {
    return await apiClient.get(`/reseller-tickets/${ticketId}`);
}

export async function updateTicket(ticketId, data) {
    return await apiClient.put(`/reseller-tickets/${ticketId}`, data);
}

export async function deleteTicket(ticketId) {
    return await apiClient.delete(`/reseller-tickets/${ticketId}`);
}

export async function getMyTickets() {
    return await apiClient.get("/reseller-tickets/my-tickets");
}

export async function getAllTickets() {
    return await apiClient.get("/reseller-tickets/all");
}

export async function getTicketsByStatus(status) {
    return await apiClient.get(`/reseller-tickets/status/${status}`);
}

export async function getTicketStatistics() {
    return await apiClient.get("/reseller-tickets/statistics");
}

export async function getReadableTickets() {
    return await apiClient.get("/reseller-tickets/readable");
}