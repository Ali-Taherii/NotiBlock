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
    return res.json()
}

export async function getRecalls() {
    const res = await fetch('https://localhost:7179/api/recall/all')
    if (!res.ok) {
        throw new Error('Failed to fetch recalls')
    }
    return res.json()
}

export async function getRecallById(id) {
    const res = await fetch(`https://localhost:7179/api/recall/${id}`)
    if (!res.ok) {
        throw new Error('Failed to fetch recall')
    }
    return res.json()
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
    return res.json()
}

export async function deleteRecall(id) {
    const res = await fetch(`https://localhost:7179/api/recall/${id}`, {
        method: 'DELETE',
    })
    if (!res.ok) {
        throw new Error('Failed to delete recall')
    }
    return res.json()
}
