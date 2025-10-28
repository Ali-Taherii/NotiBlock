import { useState, useEffect, useCallback } from "react";
import {
    getAllTickets,
    getMyTickets,
    getTicketsByUser,
    getTicketsByStatus,
    getPendingTickets,
    getApprovedTickets,
} from "../api/tickets";

export const useTickets = (options = {}) => {
    const [tickets, setTickets] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    const {
        fetchAll = false,
        fetchMy = false,
        userId = null,
        status = null,
        fetchPending = false,
        fetchApproved = false,
        autoFetch = true
    } = options;

    const fetchTickets = useCallback(async () => {
        try {
            setLoading(true);
            setError(null);

            let data;

            if (fetchAll) {
                data = await getAllTickets();
            } else if (fetchMy) {
                data = await getMyTickets();
            } else if (userId) {
                data = await getTicketsByUser(userId);
            } else if (status) {
                data = await getTicketsByStatus(status);
            } else if (fetchPending) {
                data = await getPendingTickets();
            } else if (fetchApproved) {
                data = await getApprovedTickets();
            } else {
                data = await getAllTickets(); // default
            }

            setTickets(data || []);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    }, [fetchAll, fetchMy, userId, status, fetchPending, fetchApproved]);


    const refetch = useCallback(() => {
        fetchTickets();
    }, [fetchTickets]);

    useEffect(() => {
        if (autoFetch) {
            fetchTickets();
        }
    }, [fetchTickets, autoFetch]);

    return {
        tickets,
        loading,
        error,
        refetch,
        fetchTickets
    };
};
