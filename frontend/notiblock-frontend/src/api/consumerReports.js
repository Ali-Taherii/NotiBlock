import apiClient from "./api";

export async function submitReport(data) {
    return await apiClient.post("/consumer-reports", data);
}

export async function submitReportsBulk(items) {
    return await apiClient.post("/consumer-reports/bulk", { items });
}

export async function getReportById(id) {
    return await apiClient.get(`/consumer-reports/${id}`);
}

export async function updateReport(id, data) {
    return await apiClient.put(`/consumer-reports/${id}`, data);
}

export async function deleteReport(id) {
    return await apiClient.delete(`/consumer-reports/${id}`);
}

export async function getAllReports() {
    return await apiClient.get("/consumer-reports/all");
}

export async function getMyReports() {
    return await apiClient.get(`/consumer-reports/my-reports`);
}

export async function getReportsByProductSerialNumber(serialNumber) {
    return await apiClient.get(`/consumer-reports/product/${serialNumber}`);
}

export async function getReportsByStatus(status) {
    return await apiClient.get(`/consumer-reports/status/${status}`);
}

export async function actionOnReport(id, actionData) {
    return await apiClient.post(`/consumer-reports/${id}/action`, actionData);
}

export async function getReportStatistics() {
    return await apiClient.get("/consumer-reports/statistics");
}

export async function getResellerReports() {
    return await apiClient.get("/consumer-reports/reseller/related-reports");
}