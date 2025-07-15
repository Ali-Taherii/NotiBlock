export async function createRecall(data) {
    const res = await fetch('https://localhost:7179/api/recall', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    })
    if (!res.ok) {
        throw new Error('Failed to create recall')
    }

    const json = await res.json()
    return json.data;
}

export async function getRecalls() {
    const res = await fetch('https://localhost:7179/api/recall/all')
    if (!res.ok) {
        throw new Error('Failed to fetch recalls')
    }

    const json = await res.json()
    return json.data;
}

export async function getRecallById(id) {
    const res = await fetch(`https://localhost:7179/api/recall/${id}`)
    if (!res.ok) {
        throw new Error('Failed to fetch recall')
    }

    const json = await res.json()
    return json.data;
}

export async function updateRecall(id, data) {
    const res = await fetch(`https://localhost:7179/api/recall/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    })
    if (!res.ok) {
        throw new Error('Failed to update recall')
    }

    const json = await res.json()
    return json.data;
}

export async function deleteRecall(id) {
    const res = await fetch(`https://localhost:7179/api/recall/${id}`, {
        method: 'DELETE',
    })
    if (!res.ok) {
        throw new Error('Failed to delete recall')
    }

    const json = await res.json()
    return json.data;
}
