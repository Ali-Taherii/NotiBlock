export default function IssueRecallForm() {
    return (
        <div className="bg-white shadow-md rounded-lg p-6 mb-6">
            <h2 className="text-lg font-semibold mb-4">Issue New Recall</h2>
            <form>
                <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="product">
                        Product
                    </label>
                    <input
                        type="text"
                        id="product"
                        className="border border-gray-300 rounded-lg p-2 w-full"
                        placeholder="Enter product name"
                    />
                </div>
                <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="reason">
                        Reason for Recall
                    </label>
                    <textarea
                        id="reason"
                        className="border border-gray-300 rounded-lg p-2 w-full"
                        placeholder="Enter reason for recall"
                    ></textarea>
                </div>
                <button
                    type="submit"
                    className="bg-blue-500 text-white rounded-lg px-4 py-2"
                >
                    Issue Recall
                </button>
            </form>
        </div>
    );
}
