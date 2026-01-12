export default function Notifications() {
    return (
        <div className="fixed bottom-4 right-4 bg-white shadow-lg rounded-lg p-4 w-64">
            <h2 className="text-lg font-semibold mb-2">Notifications</h2>
            <ul className="list-disc list-inside text-sm text-gray-700">
                <li>No new notifications</li>
            </ul>
        </div>
    );
}