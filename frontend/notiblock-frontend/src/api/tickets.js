import { apiFetch } from "./api";

// Get all tickets
export async function getAllTickets() {
    return await apiFetch("/ticket");
}

// Get tickets by user ID
export async function getTicketsByUser(userId) {
    return await apiFetch(`/ticket/user/${userId}`);
}

// Get tickets for the current authenticated user
export async function getMyTickets() {
    return await apiFetch("/ticket/me");
}

// Get a specific ticket by ID
export async function getTicketById(ticketId) {
    return await apiFetch(`/ticket/${ticketId}`);
}

// Create a new ticket
export async function createTicket(ticketData) {
    return await apiFetch("/ticket", {
        method: "POST",
        body: JSON.stringify(ticketData),
    });
}

// Update a ticket
export async function updateTicket(ticketId, updateData) {
    return await apiFetch(`/ticket/edit/${ticketId}`, {
        method: "PUT",
        body: JSON.stringify(updateData),
    });
}

// Update ticket status (for regulators/admins)
export async function updateTicketStatus(ticketId, status) {
    return await apiFetch(`/ticket/${ticketId}/status`, {
        method: "PATCH",
        body: JSON.stringify({ status }),
    });
}

// Delete a ticket
export async function deleteTicket(ticketId) {
    return await apiFetch(`/ticket/delete/${ticketId}`, {
        method: "DELETE",
    });
}

// Get tickets by status
export async function getTicketsByStatus(status) {
    return await apiFetch(`/ticket/status/${status}`);
}

// Get pending tickets (for regulators)
export async function getPendingTickets() {
    return await apiFetch("/ticket/status/pending");
}

// Get approved tickets (for manufacturers)
export async function getApprovedTickets() {
    return await apiFetch("/ticket/status/approved");
}

// Add a comment to a ticket
export async function addTicketComment(ticketId, comment) {
    return await apiFetch(`/ticket/${ticketId}/comment`, {
        method: "POST",
        body: JSON.stringify({ comment }),
    });
}

// Get ticket comments
export async function getTicketComments(ticketId) {
    return await apiFetch(`/ticket/${ticketId}/comments`);
}