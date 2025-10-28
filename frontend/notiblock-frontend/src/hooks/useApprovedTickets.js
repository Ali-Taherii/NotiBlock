import { useState, useEffect } from 'react'
import { getApprovedTickets } from '../api/tickets'

export function useApprovedTickets() {
    const [tickets, setTickets] = useState([])
    const [loading, setLoading] = useState(true)
    const [error, setError] = useState(null)

    const fetchTickets = async () => {
        try {
            setLoading(true)
            setError(null)
            const data = await getApprovedTickets()
            setTickets(data)
        } catch (err) {
            console.error('Error fetching approved tickets:', err)
            setError(err.message || 'Failed to fetch approved tickets')
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