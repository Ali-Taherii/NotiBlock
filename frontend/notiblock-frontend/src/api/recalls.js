import { apiFetch } from "./api";

export async function createRecall(data) {
    return await apiFetch("/recall", {
        method: "POST",
        body: JSON.stringify(data),
    });
}

export async function getRecalls() {
    return await apiFetch("/recall/all");
}

export async function getRecallById(id) {
    return await apiFetch(`/recall/${id}`);
}

export async function updateRecall(id, data) {
    return await apiFetch(`/recall/${id}`, {
        method: "PUT",
        body: JSON.stringify(data),
    });
}

export async function deleteRecall(id) {
    return await apiFetch(`/recall/${id}`, {
        method: "DELETE",
    });
}
