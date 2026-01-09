import apiClient from "./api";

export async function createProduct(data) {
    return await apiClient.post("/products/create", data);
}

export async function registerProduct(data) {
    return await apiClient.post("/products/register", data);
}

export async function unregisterProduct(data) {
    return await apiClient.post("/products/unregister", data);
}

export async function getProductBySerialNumber(serialNumber) {
    return await apiClient.get(`/products/${serialNumber}`);
}

export async function updateProduct(serialNumber, data) {
    return await apiClient.put(`/products/${serialNumber}`, data);
}

export async function deleteProduct(serialNumber) {
    return await apiClient.delete(`/products/${serialNumber}`);
}

export async function getMyProducts(role) {
    return await apiClient.get(`/products/${role}`);
}
