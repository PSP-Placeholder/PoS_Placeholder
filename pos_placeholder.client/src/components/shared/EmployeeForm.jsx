import {Button, Form, FormGroup, Input, Label, Row} from "reactstrap";
import {useState} from "react";

const EmployeeForm = ({ onSubmit, credentials, formId = '0' }) => {
    const [newCredentials, setNewCredentials] = useState(credentials);

    const handleInputChange = (e) => {
        const { id, value } = e.target;
        const field = id.replace(formId, "");
        setNewCredentials((prevState) => ({
            ...prevState,
            [field]: value,
        }));
    };

    return (
        <Form onSubmit={(e) => {
            e.preventDefault();
            onSubmit(newCredentials);
        }}>
            {Object.entries(newCredentials)
                .map(([key, value]) => (
                    <FormGroup key={formId + key}>
                        <Label for={formId + key}>
                            {key
                                .replace(/([A-Z])/g, " $1")
                                .replace(/^./, (str) => str.toUpperCase())}
                        </Label>
                        <Input
                            id={formId + key}
                            value={value}
                            onChange={handleInputChange}
                            placeholder={`Enter your ${key
                                .replace(/([A-Z])/g, " $1")
                                .toLowerCase()}`}
                            type={key.includes("password") || key.includes("confirmPassword") ? "password" : "text"}
                        />
                    </FormGroup>
                ))}
            <Row className="d-flex justify-content-center align-items-center">
                <Button color="success" className="me-3 w-25" type="submit">
                    Submit
                </Button>
                <Button color="danger" className="w-25" onClick={() => setNewCredentials(credentials)}>
                    Clear
                </Button>
            </Row>
        </Form>
    );
}

export default EmployeeForm;