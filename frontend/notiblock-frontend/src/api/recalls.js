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
    const res = await fetch('https://localhost:7179/api/recall')
    if (!res.ok) {
        throw new Error('Failed to fetch recalls')
    }
    return res.json()
}
