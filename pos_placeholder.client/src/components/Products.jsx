import {useEffect, useState} from "react";
import {
    Accordion,
    AccordionBody,
    AccordionHeader,
    AccordionItem,
    Button,
    Col, Form, FormGroup,
    Input,
    Label, Modal, ModalBody, ModalFooter, ModalHeader,
    Row
} from "reactstrap";
import * as ProductApi from "@/api/productApi.jsx";
import ProductVariationForm from "@/components/shared/ProductVariationForm.jsx";
import {getCurrency} from "@/helpers/currencyUtils.jsx";

const Products = () => {
    const [isLoading, setIsLoading] = useState(true);

    const [products, setProducts] = useState([]);
    const [variations, setVariations] = useState([]);

    const [itemName, setItemName] = useState('');
    const [itemGroup, setItemGroup] = useState('');

    const [editProductName, setEditProductName] = useState('');
    const [editProductGroup, setEditProductGroup] = useState('');

    const [isAddVariationFormOpen, setIsAddVariationFormOpen] = useState(false);
    const [editVariationFormOpenFor, setEditVariationFormOpenFor] = useState('');
    const [editProductFormOpenFor, setEditProductFormOpenFor] = useState('');

    const [open, setOpen] = useState(null);

    const fetchProducts = async () => {
        const fetchedProducts = await ProductApi.getProducts();
        setProducts(fetchedProducts);
        setIsLoading(false);
    };

    const fetchProductVariations = async (id) => {
        const fetchedProductVariations = await ProductApi.getProductVariations(id);
        setVariations(fetchedProductVariations);
    };


    useEffect( () => {
        fetchProducts();
    },[]);

    const toggleAccordion = async (id) => {
        if (open === id) {
            setOpen(null);
            setVariations([])
        } else {
            setOpen(id);
            setIsAddVariationFormOpen(false);
            await fetchProductVariations(id);
        }
    };

    const removeProduct = async (id) => {
        await ProductApi.deleteProduct(id);
        setProducts(products.filter(product => product.id !== id));
    }

    const removeVariation = async (id) => {
        await ProductApi.deleteProductVariation(id);
        setVariations(variations.filter(variation => variation.id !== id));
    }

    const addProduct = async () => {
        const newProduct = new FormData();
        newProduct.append("name", itemName);
        newProduct.append("itemGroup", itemGroup);
        const createdProduct = await ProductApi.addProduct(newProduct);
        setProducts([...products, createdProduct]);
    };

    const updateProduct = async (item) => {
        const updatedProduct = new FormData();

        updatedProduct.append("id", item.id);
        if (editProductName !== '')
            updatedProduct.append("name", editProductName);
        if (editProductGroup !== '')
            updatedProduct.append("itemGroup", editProductGroup);

        const newProduct = await ProductApi.updateProduct(updatedProduct);
        setProducts(
            products.map((v) => (v.id === newProduct.id ? newProduct : v))
        );
        handleClearEditProduct();
    };

    const handleClearProduct = () => {
        setItemName('');
        setItemGroup('');
    };

    const handleClearEditProduct = () => {
        setEditProductName('');
        setEditProductGroup('');
        setEditProductFormOpenFor('');
    };


    const handleAddVariation = async (productId, formData) => {
        formData.append("productId", productId);
        const createdVariation = await ProductApi.addProductVariation(formData);
        setVariations([...variations, createdVariation]);
        setIsAddVariationFormOpen(false);
    };

    const handleUpdateVariation = async (formData) => {
        const updatedVariation = await ProductApi.updateProductVariation(formData);
        console.log(formData);
        setVariations(
            variations.map((v) => (v.id === updatedVariation.id ? updatedVariation : v))
        );
        setEditVariationFormOpenFor('');
    };

    if (isLoading) {
        return <div>Loading...</div>;
    }

    const editProductForm = () => {
        const item = products.find(p => p.id === editProductFormOpenFor);
        if (!item) return null;
        return (
            <Modal isOpen={editProductFormOpenFor !== ''} fade={false} size="lg" centered={true}>
                <ModalHeader>
                    {item.name} in group {item.itemGroup}
                </ModalHeader>
                <ModalBody>
                    <Form>
                        <FormGroup row>
                            <Label sm={3}>Product Name</Label>
                            <Col sm={9}>
                                <Input
                                    value={editProductName}
                                    onChange={(e) => setEditProductName(e.target.value)}
                                />
                            </Col>
                        </FormGroup>
                        <FormGroup row>
                            <Label sm={3}>Product group</Label>
                            <Col sm={9}>
                                <Input
                                    value={editProductGroup}
                                    onChange={(e) => setEditProductGroup(e.target.value)}
                                />
                            </Col>
                        </FormGroup>
                    </Form>
                </ModalBody>
                <ModalFooter>
                    <Button color="success" className="me-3" onClick={() => updateProduct(item)}>
                        Update
                    </Button>
                    <Button color="danger" onClick={handleClearEditProduct}>
                        Cancel
                    </Button>
                </ModalFooter>
            </Modal>
        );
    };


    return (
        <Row style={{ height: "85vh" }}>
            <Col className="border rounded shadow-sm m-2 p-0 d-flex flex-column">
                <h4 className="p-2 d-flex justify-content-center">All products</h4>
                <Accordion open={open} toggle={toggleAccordion}>
                    {products.map((item) => (
                        <AccordionItem key={item.id}>
                            <AccordionHeader targetId={item.id.toString()}>
                                <div className="d-flex justify-content-between w-100 me-3">
                                    {item.name}
                                    <div>
                                        <Button
                                            color="secondary"
                                            outline
                                            onClick={() => setEditProductFormOpenFor(item.id)}
                                            className="me-3"
                                        >
                                            <i className="bi-pencil"></i>
                                        </Button>
                                        <Button
                                            color="danger"
                                            onClick={() => removeProduct(item.id)}
                                            className="ms-auto"
                                        >
                                            <i className="bi-trash"></i>
                                        </Button>
                                    </div>
                                </div>
                            </AccordionHeader>
                            <AccordionBody accordionId={item.id.toString()}>
                                {variations.map((variation) => (
                                    <div key={variation.id}>
                                        <Row className="mb-2 align-items-center">
                                            <Col xs="auto">
                                                <img
                                                    src={variation.pictureUrl}
                                                    alt={variation.name}
                                                    style={{ width: "50px" }}
                                                />
                                            </Col>
                                            <Col>
                                                <span>{variation.name} - {getCurrency()}{variation.price} | Stock: {variation.inventoryQuantity}</span>
                                            </Col>
                                            <Col xs="auto">
                                                <Button
                                                    color="secondary"
                                                    outline
                                                    onClick={() => setEditVariationFormOpenFor(variation.id)}
                                                >
                                                    <i className="bi-pencil"></i>
                                                </Button>
                                            </Col>
                                            <Col xs="auto">
                                                <Button color="danger" onClick={() => removeVariation(variation.id)}>
                                                    <i className="bi-trash"></i>
                                                </Button>
                                            </Col>
                                        </Row>
                                        {editVariationFormOpenFor === variation.id && (
                                            <ProductVariationForm
                                                variation={variation}
                                                onSubmit={(formData) =>
                                                    handleUpdateVariation(formData)
                                                }
                                                onCancel={() => setEditVariationFormOpenFor('')}
                                            />
                                        )}
                                    </div>
                                ))}
                                {isAddVariationFormOpen ? (
                                    <ProductVariationForm
                                        onSubmit={(formData) =>
                                            handleAddVariation(item.id, formData)
                                        }
                                        onCancel={() => setIsAddVariationFormOpen(false)}
                                    />
                                ) : (
                                    <Row>
                                        <Button color="secondary" outline onClick={() => setIsAddVariationFormOpen(true)}>
                                            Add variation
                                        </Button>
                                    </Row>
                                )}
                            </AccordionBody>
                        </AccordionItem>
                    ))}
                </Accordion>
                {editProductForm()}
            </Col>
            <Col className="border rounded shadow-sm m-2 p-4">
                <h4 className="p-2 d-flex justify-content-center">Create new product</h4>
                <div className="d-flex justify-content-center mb-1">
                    <Label className="w-25 d-flex flex-column justify-content-center p-0 m-0">Item name</Label>
                    <Input placeholder="Enter item name"
                        value={itemName}
                        onChange={(e) => setItemName(e.target.value)}>
                    </Input>
                </div>
                <div className="d-flex justify-content-center mb-1">
                    <Label className="w-25 d-flex flex-column justify-content-center p-0 m-0">Item group</Label>
                    <Input placeholder="Enter item group"
                           value={itemGroup}
                           onChange={(e) => setItemGroup(e.target.value)}>
                    </Input>
                </div>
                <div className="d-flex justify-content-center mt-5">
                    <Button color="success" className="me-3" onClick={addProduct}>
                        Create
                    </Button>
                    <Button color="danger" onClick={handleClearProduct}>
                        Clear
                    </Button>
                </div>
            </Col>
        </Row>
    );
}

export default Products;