import React, {useState} from "react";
import {Button, Input, Label, Form, FormGroup, FormFeedback} from "reactstrap";
import * as orderApi from "@/api/orderApi.jsx";

const Giftcard = ({onPaymentSuccess, order, tip}) => {
    const [giftcardId, setGiftcardId] = useState("");
    const [errorMessage, setErrorMessage] = useState("");
    const [isValid, setIsValid] = useState(true);

    const validateGiftCardId = (id) => {
        // GUID: 8-4-4-4-12
        const guidRegex = /^[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}$/;
        return guidRegex.test(id);
    };

    const handleSubmit = async () => {
        setErrorMessage("");
        setIsValid(true);

        if (!giftcardId.trim()) {
            setErrorMessage("Giftcard ID cannot be empty.");
            setIsValid(false);
            return;
        }
        if (!validateGiftCardId(giftcardId)) {
            setErrorMessage("Invalid Giftcard ID format. Expected format: {8}-{4}-{4}-{4}-{12}.");
            setIsValid(false);
            return;
        }

        try {
            const createOrderDto = {
                Tip: tip ? Number(tip) : null,
                OrderItems: order.products.map((item) => ({
                    ProductVariationId: item.productVariationId,
                    Quantity: item.quantity,
                })),
                PaymentIntentId: null,
                GiftCardId: giftcardId,
                Method: 1,
            };
            
            await orderApi.createOrder(createOrderDto);

            // If successful createOrder
            onPaymentSuccess();
        } catch (error) {
            const backendError = error.message || "An unknown error occurred.";
            setErrorMessage(backendError);
            setIsValid(false);
        }
    };

    return (
        <FormGroup className="d-flex align-items-center">
            <Label className="me-3 mb-0">Giftcard:</Label>
            <div className="flex-grow-1">
                <Input
                    type="text"
                    placeholder="Enter gift card code"
                    value={giftcardId}
                    invalid={!isValid}
                    onChange={(e) => {
                        setGiftcardId(e.target.value);
                        setErrorMessage("");
                        setIsValid(true);
                    }}
                />
                <FormFeedback className="position-absolute">{errorMessage}</FormFeedback>
            </div>
            <Button color="success" className="ms-3" onClick={handleSubmit}>
                Pay
            </Button>
        </FormGroup>
    );
};

export default Giftcard;
