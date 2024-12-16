import {useStripe, useElements, PaymentElement} from '@stripe/react-stripe-js';
import React from 'react';
import * as orderApi from "@/api/orderApi.jsx";

const CheckoutForm = ({paymentData, order, tip, onPaymentSuccess}) => {
    const stripe = useStripe();
    const elements = useElements();

    const handleSubmit = async (event) => {
        event.preventDefault();

        if (!stripe || !elements) {
            return;
        }

        const result = await stripe.confirmPayment({
            elements,
            confirmParams: {
                return_url: "https://example.com/order/123/complete",
            },
            redirect: "if_required"
        });

        if (result.error) {
            alert(result.error.message);
        } else {
            const createOrderDto = {
                Tip: tip ? Number(tip) : null,
                OrderItems: order.products.map(item => ({
                    ProductVariationId: item.productVariationId,
                    Quantity: item.quantity
                })),
                PaymentIntentId: paymentData.paymentIntentId,
                GiftCardId: null,
                Method: 0 // 0 -> "card", 1 -> "giftcard", 2 -> "cash"
            };
            const createdOrder = await orderApi.createOrder(createOrderDto);
            console.log(createdOrder);
            onPaymentSuccess();
        }
    };

    return (
        <form onSubmit={handleSubmit}>
            <PaymentElement/>
            <button className="btn btn-success mt-4 w-100" disabled={!stripe}>Pay</button>
        </form>
    );
};

export default CheckoutForm;