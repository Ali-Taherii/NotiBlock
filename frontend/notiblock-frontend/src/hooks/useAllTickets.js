import { useState, useEffect } from 'react'
import { getAllTickets } from '../api/tickets'

export function useAllTickets() {
    const [tickets, setTickets] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const fetchTickets = async () => {
        try {
            setLoading(true)
            setError(null)
            const data = await getAllTickets()
            setTickets(data)
        } catch (err) {
            console.error('Error fetching tickets:', err)
            setError(err.message || 'Failed to fetch tickets')
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        fetchTickets()
    }, [])

    const refetch = () => {
        fetchTickets()
    }

    return { tickets, loading, error, refetch }
}