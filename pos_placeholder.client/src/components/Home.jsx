﻿import {Button, Col, Input, Label, Modal, ModalBody, ModalFooter, ModalHeader, Row} from "reactstrap";
import {useEffect, useState} from "react";
import * as productApi from "@/api/productApi.jsx";
import * as orderApi from "@/api/orderApi.jsx";
import {getCurrency} from "@/helpers/currencyUtils.jsx";

const Home = () => {
    const [isLoading, setIsLoading] = useState(true);
    
    const [tip, setTip] = useState('');
    const [orderPreview, setOrderPreview] = useState([]);
    const [order, setOrder] = useState({products: []});

    const [totalPrice, setTotalPrice] = useState("0");
    const [products, setProducts] = useState(null);
    const [variations, setVariations] = useState([]);
    const [productsInCart, setProductsInCart] = useState(null);
    const [productsInCatalogue, setProductsInCatalogue] = useState(null);
    
    const [selectedProduct, setSelectedProduct] = useState(null);
    const [paySelected, setPaySelected] = useState(false);

    const fetchProducts = async () => {
        try {
            const fetchedProducts = await productApi.getProducts();
            setProducts(fetchedProducts);
        } catch (error) {
            console.error("Error fetching products:", error);
        } finally {
            setIsLoading(false);
        }
    };

    const fetchProductVariations = async (id) => {
        const fetchedProductVariations = await productApi.getProductVariations(id);
        setVariations(fetchedProductVariations);
    };

    useEffect(() => {
        fetchProducts();
    }, []);

    useEffect(() => {
        if (products && products.length > 0) {
            formatProductsInCatalogue(products);
        }
    }, [products]);

    useEffect(() => {
        formatProductsInCart();
        const total = order.products.reduce((acc, item) => acc + item.price * item.quantity, 0);
        setTotalPrice(total.toFixed(2));
    }, [order]);

    const handleAddToCart = (variation, product) => {
        const newProductInCart = {
            productVariationId: variation.id,
            fullName: product.name + " " + variation.name,
            price: variation.price,
            quantity: 1,
        };

        const existingProductIndex = order.products.findIndex(item => item.productVariationId === variation.id);
        let updatedProductsInCart;
        if (existingProductIndex === -1) {
            updatedProductsInCart = [...order.products, newProductInCart];
        } else {
            updatedProductsInCart = order.products.map((item, index) => {
                if (index === existingProductIndex) {
                    return {...item, quantity: item.quantity + 1};
                }
                return item;
            });
        }

        setOrder(prevOrder => ({...prevOrder, products: updatedProductsInCart}));
    };

    const handleRemoveFromCart = (productVariationId) => {
        const existingIndex = order.products.findIndex(item => item.productVariationId === productVariationId);
        if (existingIndex === -1) {
            return;
        }

        const existingItem = order.products[existingIndex];
        let updatedProducts;

        if (existingItem.quantity > 1) {
            updatedProducts = order.products.map((item, index) => {
                if (index === existingIndex) {
                    return {...item, quantity: item.quantity - 1};
                }
                return item;
            });
        } else {
            updatedProducts = order.products.filter(item => item.productVariationId !== productVariationId);
        }

        setOrder((prevOrder) => ({...prevOrder, products: updatedProducts}));
    };

    const handlePayNowClick = async () => {
        if (order.products.length === 0) {
            alert("Your cart is empty. Please add items before paying.");
            return;
        }

        const createOrderDto = {
            Tip: tip ? Number(tip) : null,
            OrderItems: order.products.map(item => ({
                ProductVariationId: item.productVariationId,
                Quantity: item.quantity
            }))
        };

        const orderPreviewResponse = await orderApi.getOrderPreview(createOrderDto);
        setOrderPreview(orderPreviewResponse);
        setPaySelected(true);
    };
    
    const handlePayment = async() => {
        const createOrderDto = {
            Tip: tip ? Number(tip) : null,
            OrderItems: order.products.map(item => ({
                ProductVariationId: item.productVariationId,
                Quantity: item.quantity
            }))
        };
        
        setOrder({products: []});
        const createdOrder = await orderApi.createOrder(createOrderDto);
        console.log(createdOrder);
    }

    const handleProductClick = async (product) => {
        setSelectedProduct(product);
        await fetchProductVariations(product.id);
    };

    const formatProductsInCart = () => {
        const formatedProductsInCart = order.products.map((item, index) => (
            <Row key={index} className="p-2">
                <Col>{item.fullName}</Col>
                <Col className="d-flex justify-content-center">x{item.quantity}</Col>
                <Col className="d-flex justify-content-end">
                    {item.price} {getCurrency()}
                    <i
                        className="bi-x-circle px-2"
                        style={{cursor: "pointer"}}
                        onClick={() => handleRemoveFromCart(item.productVariationId)}
                    ></i>
                </Col>
            </Row>
        ));

        setProductsInCart(formatedProductsInCart);
    };

    const formatProductsInCatalogue = (products) => {
        const itemsPerRow = 6;
        const groupedProducts = products.reduce((acc, product) => {
            acc[product.itemGroup] = acc[product.itemGroup] || [];
            acc[product.itemGroup].push(product);
            return acc;
        }, {});

        const catalogue = Object.entries(groupedProducts).map(([groupName, groupProducts]) => (
            <div key={groupName}>
                <div className="mb-3">{groupName}</div>
                {Array.from({length: Math.ceil(groupProducts.length / itemsPerRow)}, (_, rowIndex) => {
                    const rowItems = groupProducts.slice(rowIndex * itemsPerRow, (rowIndex + 1) * itemsPerRow);

                    return (
                        <Row key={rowIndex} className="pb-4">
                            {rowItems.map((item) => (
                                <Col key={item.id} md={12 / itemsPerRow} xs={6}>
                                    <div className="border rounded p-2" style={{cursor: "pointer"}}
                                         onClick={() => handleProductClick(item)}>
                                        {item.name}
                                    </div>
                                </Col>
                            ))}
                        </Row>
                    );
                })}
            </div>
        ));

        setProductsInCatalogue(catalogue);
    }

    const modal =
        <Modal isOpen={!!selectedProduct} fade={false} size="lg" centered={true}>
            <ModalBody>
                <h5>{selectedProduct?.name}</h5>
                {variations.map((variation) => (
                    <div key={variation.id} className="p-2 border rounded mb-2"
                         onClick={() => handleAddToCart(variation, selectedProduct)}>
                        <h6>{variation.name}</h6>
                        <p>Price: {variation.price} {getCurrency()}</p>
                        {variation.pictureUrl && <img src={variation.pictureUrl} alt={variation.name} width="50"/>}
                    </div>
                ))}
            </ModalBody>
            <ModalFooter>
                <Button color="secondary" onClick={() => setSelectedProduct(null)}>
                    Cancel
                </Button>
            </ModalFooter>
        </Modal>

    if (isLoading) {
        return <div>Loading...</div>;
    }

    const paymentModal =
        <Modal isOpen={paySelected} fade={false} size="lg" centered={true}>
            <ModalHeader>
                Checkout
            </ModalHeader>
            <ModalBody>
                <div>
                    <div className="d-flex justify-content-between">
                        <span>Subtotal:</span>
                        <span>{orderPreview.subTotal} €</span>
                    </div>
                    <div className="d-flex justify-content-between">
                        <span>Taxes:</span>
                        <span>{orderPreview.taxesTotal} €</span>
                    </div>
                    <div className="d-flex justify-content-between">
                        <span>Tip:</span>
                        <span>{orderPreview.tip} €</span>
                    </div>
                    <hr/>
                    <div className="d-flex justify-content-between fw-bold">
                        <span>Total:</span>
                        <span>{orderPreview.total} €</span>
                    </div>
                </div>
                <div>
                    Card payment fields from stripe
                </div>
            </ModalBody>
            <ModalFooter>
                <Button color="danger" className="w-25" onClick={() => setPaySelected(false)}>
                    Cancel
                </Button>
                <Button color="success" className="w-25" onClick={handlePayment}>
                    Pay
                </Button>
            </ModalFooter>
        </Modal>

    return (
        <Row style={{height: "85vh"}}>
            <Col className="border rounded shadow-sm m-2 p-1 d-flex flex-column" lg={4}>
                <div className="justify-content-center border rounded p-2">
                    Total: {totalPrice} {getCurrency()}
                </div>
                <div>
                    {productsInCart}
                </div>
                <div className="d-flex justify-content-center m-2 mt-auto align-items-center">
                    <div className="d-flex justify-content-start col-xl-6 col-lg-4">
                        <Label className="w-25 d-flex flex-column justify-content-center p-0 m-0">Tip:</Label>
                        <Input placeholder="Enter tip value"
                               value={tip}
                               type={"number"}
                               onChange={(e) => setTip(e.target.value)}>
                        </Input>
                    </div>
                    <div className="d-flex justify-content-center col-auto">
                        <Button color="dark" outline className="m-1">Split order</Button>
                        <Button color="success" className="m-1" onClick={handlePayNowClick}>Pay now</Button>
                    </div>
                </div>
            </Col>
            <Col className="border rounded shadow-sm m-2 p-2">
                {productsInCatalogue}
            </Col>

            {modal}
            {paymentModal}
        </Row>
    );
};

export default Home;