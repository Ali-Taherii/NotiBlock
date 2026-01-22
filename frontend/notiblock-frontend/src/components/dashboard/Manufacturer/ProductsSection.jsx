import { useState, useEffect, useCallback } from 'react';
import { createProduct, getMyProducts, deleteProduct } from '../../../api/products';
import { FiPlus, FiTrash2, FiPackage } from 'react-icons/fi';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

export default function ProductsSection() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [formData, setFormData] = useState({
    serialNumber: '',
    model: '',
  });
  const { toast, success, error, hideToast } = useToast();

  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getMyProducts('manufacturer');
      const items = response?.data?.items || response?.items || response || [];
      setProducts(Array.isArray(items) ? items : []);
    } catch (err) {
      console.error('Error fetching products:', err);
      error('Failed to load products');
    } finally {
      setLoading(false);
    }
  }, [error]);

  useEffect(() => {
    fetchProducts();
  }, [fetchProducts]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const response = await createProduct(formData);
      success(response?.message || 'Product created successfully!');
      setFormData({ serialNumber: '', model: '' });
      setShowForm(false);
      fetchProducts();
    } catch (err) {
      console.error('Error creating product:', err);
      error(err.message || 'Failed to create product');
    }
  };

  const handleDelete = async (serialNumber) => {
    if (!window.confirm('Are you sure you want to delete this product?')) return;

    try {
      await deleteProduct(serialNumber);
      success('Product deleted successfully!');
      fetchProducts();
    } catch (err) {
      console.error('Error deleting product:', err);
      error('Failed to delete product');
    }
  };

  if (loading) {
    return <div className="text-center py-8">Loading products...</div>;
  }

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold">My Products</h2>
        <button
          onClick={() => setShowForm(!showForm)}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          <FiPlus /> Create Product
        </button>
      </div>

      {showForm && (
        <div className="mb-6 bg-white p-6 rounded-lg shadow border">
          <h3 className="text-lg font-semibold mb-4">Create New Product</h3>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="block mb-1 font-medium">Serial Number</label>
              <input
                type="text"
                value={formData.serialNumber}
                onChange={(e) => setFormData({ ...formData, serialNumber: e.target.value })}
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                required
                placeholder="e.g., ABC-123-XYZ"
              />
            </div>
            <div>
              <label className="block mb-1 font-medium">Model</label>
              <input
                type="text"
                value={formData.model}
                onChange={(e) => setFormData({ ...formData, model: e.target.value })}
                className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                required
                placeholder="e.g., Toaster X200"
              />
            </div>
            <div className="flex gap-3">
              <button
                type="submit"
                className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
              >
                Create Product
              </button>
              <button
                type="button"
                onClick={() => setShowForm(false)}
                className="px-4 py-2 bg-gray-500 text-white rounded hover:bg-gray-600"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {products.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg border border-dashed border-gray-300">
          <FiPackage className="mx-auto text-gray-400 text-5xl mb-3" />
          <p className="text-gray-600">No products created yet.</p>
          <p className="text-sm text-gray-500 mt-1">Create your first product to get started.</p>
        </div>
      ) : (
        <div className="bg-white rounded-lg shadow overflow-hidden">
          <table className="min-w-full divide-y divide-gray-200">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Serial Number</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Model</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Reseller</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Owner</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Created</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Actions</th>
              </tr>
            </thead>
            <tbody className="bg-white divide-y divide-gray-200">
              {products.map((product) => (
                <tr key={product.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 whitespace-nowrap font-medium">{product.serialNumber}</td>
                  <td className="px-6 py-4 whitespace-nowrap">{product.model}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {product.resellerId ? (
                      <span className="text-green-600">Assigned</span>
                    ) : (
                      <span className="text-gray-400">Not assigned</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {product.ownerId ? (
                              <span className="text-blue-600">{product.owner.name}</span>
                    ) : (
                      <span className="text-gray-400">Not owned</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {new Date(product.registeredAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <button
                      onClick={() => handleDelete(product.serialNumber)}
                      className="text-red-600 hover:text-red-800"
                      title="Delete product"
                    >
                      <FiTrash2 />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
