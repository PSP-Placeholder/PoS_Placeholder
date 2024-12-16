import { useState } from "react";
import { Button, Col, Form, FormGroup, Input, Label, Row } from "reactstrap";
import PropTypes from "prop-types";

const ProductVariationForm = ({variation = null, onSubmit, onCancel}) => {
    const [name, setName] = useState(variation?.name || "");
    const [price, setPrice] = useState(variation?.price || "");
    const [image, setImage] = useState(null);

    const handleSubmit = () => {
        const formData = new FormData();
        if (variation) {
            formData.append("id", variation.id);
            formData.append("productId", variation.productId);
        }
        formData.append("name", name);
        formData.append("price", price);
        if (image) formData.append("pictureFile", image);
        onSubmit(formData);
    };

    return (
        <Row className="border rounded p-3 ps-4">
            <Form>
                <FormGroup row>
                    <Label sm={3}>Variation Name</Label>
                    <Col sm={9}>
                        <Input
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                        />
                    </Col>
                </FormGroup>
                <FormGroup row>
                    <Label sm={3}>Variation Price</Label>
                    <Col sm={9}>
                        <Input
                            value={price}
                            onChange={(e) => setPrice(e.target.value)}
                        />
                    </Col>
                </FormGroup>
                <FormGroup row>
                    <Label sm={3}>Picture</Label>
                    <Col sm={9}>
                        <Input
                            type="file"
                            onChange={(e) => setImage(e.target.files[0])}
                        />
                    </Col>
                </FormGroup>
            </Form>
            <Col>
                <Button color="success" className="me-3" onClick={handleSubmit}>
                    {variation ? "Update" : "Create"}
                </Button>
                <Button color="danger" onClick={onCancel}>
                    Cancel
                </Button>
            </Col>
        </Row>
    );
};

ProductVariationForm.propTypes = {
    variation: PropTypes.shape({
        id: PropTypes.number,
        name: PropTypes.string,
        price: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
        pictureUrl: PropTypes.string,
    }),
    onSubmit: PropTypes.func.isRequired,
    onCancel: PropTypes.func.isRequired,
};

export default ProductVariationForm;
