import { apiService } from "./ApiService";

export const getDiscounts = async () => {
    try {
        return await apiService.get("/api/discounts");
    } catch (e) {
        console.error("Error fetching discounts:", e);
    }
};

export const getDiscount = async (id) => {
    try {
        return await apiService.get(`/api/discounts/${id}`);
    } catch (e) {
        console.error("Error fetching discount:", e);
    }
};

export const getVariationsByDiscountId = async (id) => {
    try {
        return await apiService.get(`/api/discounts/productVariations/${id}`);
    } catch (e) {
        console.error("Error fetching discount's variations:", e);
    }
};

export const createDiscount = async (discount) => {
    try {
        return await apiService.post("/api/discounts", discount);
    } catch (e) {
        console.error("Error creating discount:", e);
    }
};

export const updateDiscount = async (discount) => {
    try {
        return await apiService.put("/api/discounts", discount);
    } catch (e) {
        console.error("Error creating discount:", e);
    }
};

export const addProductVariationsToDiscount = async (id, dto) => {
    try {
        return await apiService.put(`/api/discounts/${id}`, dto);
    } catch (e) {
        console.error("Error updating discount:", e);
    }
};


export const deleteDiscount = async (id) => {
    try {
        return await apiService.delete(`/api/discounts/${id}`);
    } catch (e) {
        console.error("Error deleting discount:", e);
    }
};

// export const getDiscounts = async () => {
//     const discountsDTO = [
//         {
//             id: 0,
//             amount: 15,
//             startDate: "2024-1-2",
//             endDate: "2024-5-2",
//             isPercentage: true
//         },
//         {
//             id: 1,
//             amount: 10,
//             startDate: "2024-10-10",
//             endDate: "2024-11-11",
//             isPercentage: false
//         }
//     ]
//
//     return discountsDTO;
// }

