import {apiService} from "./ApiService";

export const createOrder = async (products) => {
    try {
        return await apiService.post("/api/orders", products);
    } catch (e) {
        throw e;
    }
};

export const getOrderPreview = async (products) => {
    try {
        return await apiService.post("/api/orders/preview", products);
    } catch (e) {
        console.log("Error creating order preview:", e);
    }
};

export const getAllOrders = async () => {
    try {
        return await apiService.get("/api/orders");
    } catch (e) {
        throw e;
    }
};

export const getOrderById = async (orderId) => {
    try {
        return await apiService.get(`/api/orders/${orderId}`);
    } catch (e) {
        console.log("Error getting order by id:", e);
    }
};

