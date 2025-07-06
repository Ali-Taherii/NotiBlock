import { useState, useEffect } from "react";
import { getRecalls } from "../api/recalls";

export default function useRecalls() {

    const [recalls, setRecalls] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {
        const fetchRecalls = async () => {
            try {
                const data = await getRecalls();
                setRecalls(data);
            } catch (err) {
                setError(err.message);
            } finally {
                setLoading(false);
            }
        };

        fetchRecalls();
    }, []);

    return { recalls, loading, error };
}