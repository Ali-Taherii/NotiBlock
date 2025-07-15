import { useState, useEffect, useCallback } from 'react'
import { getRecalls } from '../api/recalls'

export default function useRecalls() {
    const [recalls, setRecalls] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const fetchRecalls = useCallback(() => {
        setLoading(true)
        getRecalls()
            .then(setRecalls)
            .catch(err => setError(err.message))
            .finally(() => setLoading(false))
    }, [])

    console.log(recalls.map(recall => recall.productId));

    useEffect(() => {
        fetchRecalls()
    }, [fetchRecalls])

    return { recalls, loading, error, refetch: fetchRecalls }
}
