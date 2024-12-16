import {Button, Col, Input, Label, Modal, ModalBody, ModalFooter, ModalHeader, Row, Form, FormGroup} from "reactstrap";
import * as orderApi from "@/api/orderApi.jsx";
import * as paymentApi from "@/api/paymentApi.jsx";
import {useEffect, useState} from "react";
import {getCurrency} from "@/helpers/currencyUtils.jsx";

const Orders = () => {
    const [allOrders, setAllOrders] = useState([]);
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [isLoading, setIsLoading] = useState(true);


    const fetchAllOrders = async () => {
        try {
            const fetchedOrders = await orderApi.getAllOrders();
            setAllOrders(fetchedOrders);
        } catch (error) {
            console.error("Error fetching all orders:", error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        fetchAllOrders();
    }, [])

    const handleRefund = async (orderId) => {
        try {
            const refundedOrder = await paymentApi.makeRefund(orderId);
            setAllOrders(prev => prev.map(o => o.id === orderId ? refundedOrder : o));
        } catch (e) {
            console.log(e);
            alert("Failed to refund the order. Please try again.");
        }
    }

    const handleViewReceipt = async (orderId) => {
        const order = allOrders.find(o => o.id === orderId);
        setSelectedOrder(order);
    }

    if (isLoading) {
        return <div>Loading...</div>;
    }

    return (
        <Row style={{height: "85vh"}}>
            <Col className="border rounded shadow-sm m-2 p-2 d-flex flex-column" xl={4}>
                <div className="justify-content-center border shadow-sm rounded p-2 mb-2">
                    <h4 className="d-flex justify-content-center">All Business Orders</h4>
                </div>
                {allOrders.map((order, index) => (
                    <div key={index} className="justify-content-center border rounded">
                        <Row className="p-2 align-items-center">
                            <Col xs={12} sm={7} className="d-flex align-items-center mb-2 mb-md-0">
                                <Col className="me-3">Order #{order.id}</Col>
                                <Col className="me-3">{getCurrency()} {order.totalPrice}</Col>
                                <Col className="fw-bold fst-italic">{order.status}</Col>
                            </Col>
                            <Col xs={12} sm={5}
                                 className="d-flex justify-content-start justify-content-sm-end gap-2 mt-2 mt-md-0">
                                {order.status !== "Refunded" && (
                                    <Button color="danger" size="sm"
                                            onClick={() => handleRefund(order.id)}>Refund</Button>
                                )}
                                <Button color="warning" size="sm" onClick={() => handleViewReceipt(order.id)}>View
                                    receipt</Button>
                            </Col>
                        </Row>
                    </div>
                ))}
            </Col>

            <Col className="border rounded shadow-sm m-2 p-2">
                <div className="justify-content-center border shadow-sm rounded p-2 mb-2">
                    <h3 className="d-flex justify-content-center">Order receipt</h3>
                </div>
                {selectedOrder ? (
                    <div>
                        <div className="justify-content-center border shadow-sm rounded mb-2 p-2">
                            <div className="row">
                                <div className="col-sm-2 col-4 text-start">Subtotal:</div>
                                <div className="col-auto text-start">{getCurrency()}{selectedOrder.subTotal}</div>
                            </div>
                            <div className="row">
                                <div className="col-sm-2 col-4 text-start">Taxes:</div>
                                <div className="col-auto text-start">{getCurrency()}{selectedOrder.taxesTotal}</div>
                            </div>
                            <div className="row">
                                <div className="col-sm-2 col-4 text-start">Tip:</div>
                                <div className="col-auto text-start">{getCurrency()}{selectedOrder.tip}</div>
                            </div>
                            <div className="row">
                                <div className="col-sm-2 col-4 text-start">Discounts:</div>
                                <div className="col-auto text-start">-{getCurrency()}{selectedOrder.discountsTotal}</div>
                            </div>
                            <hr/>
                            <div className="row fw-bold">
                                <div className="col-sm-2 col-4 text-start">Total:</div>
                                <div className="col-auto text-start">{getCurrency()}{selectedOrder.totalPrice}</div>
                            </div>
                        </div>
                        <div className="justify-content-center border shadow-sm rounded p-2">
                            <h4 className="d-flex justify-content-center mb-3">Products</h4>
                            {selectedOrder.products.map((p, index) => (
                                <Row key={index} className="align-items-center border rounded p-2 m-0">
                                    <Col className="d-flex justify-content-center">{p.fullName}</Col>
                                    <Col className="d-flex justify-content-center">{getCurrency()}{p.price}</Col>
                                    <Col className="d-flex justify-content-center">x{p.quantity}</Col>
                                </Row>
                            ))}
                        </div>
                    </div>
                ) : (<div className="d-flex justify-content-center border shadow-sm rounded mb-2 p-2">No order selected.
                    Please
                    click "View receipt" on an order.</div>)}
            </Col>
        </Row>
    );
};

export default Orders;