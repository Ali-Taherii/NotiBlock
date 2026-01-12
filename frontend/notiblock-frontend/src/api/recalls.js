import apiClient from "./api";

export async function createRecall(data) {
    return await apiClient.post("/recalls", data);
}

export async function getRecalls() {
    return await apiClient.get("/recalls");
}

export async function getRecallById(id) {
    return await apiClient.get(`/recalls/${id}`);
}

export async function updateRecall(id, data) {
    return await apiClient.put(`/recalls/${id}`, data);
}

export async function deleteRecall(id) {
    return await apiClient.delete(`/recalls/${id}`);
}

export async function getMyRecalls() {
    return await apiClient.get("/recalls/manufacturer");
}