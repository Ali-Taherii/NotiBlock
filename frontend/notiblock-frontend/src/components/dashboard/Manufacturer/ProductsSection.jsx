import { useState, useEffect, useCallback } from 'react';
import { createProduct, createProductsBulk, getMyProducts, deleteProduct, unregisterProduct } from '../../../api/products';
import { FiPlus, FiTrash2, FiPackage, FiUserMinus } from 'react-icons/fi';
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
  const [useBulk, setUseBulk] = useState(false);
  const [bulkInput, setBulkInput] = useState('');
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
      if (useBulk) {
        const items = bulkInput
          .split('\n')
          .map((line) => line.trim())
          .filter(Boolean)
          .map((line) => {
            const [serialNumber, ...modelParts] = line.split(',').map((part) => part.trim());
            return {
              serialNumber,
              model: modelParts.join(',')
            };
          })
          .filter((item) => item.serialNumber && item.model);

        if (!items.length) {
          error('Please add valid lines in format: SERIAL_NUMBER,MODEL');
          return;
        }

        const response = await createProductsBulk(items);
        const result = response?.data;
        success(`Bulk creation completed: ${result?.succeeded ?? 0} succeeded, ${result?.failed ?? 0} failed.`);
        setBulkInput('');
      } else {
        const response = await createProduct(formData);
        success(response?.message || 'Product created successfully!');
        setFormData({ serialNumber: '', model: '' });
      }

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
      error(err.response?.data?.message || err.message || 'Failed to delete product');
    }
  };

  const handleRemoveReseller = async (serialNumber) => {
    if (!window.confirm('Remove the reseller from this product?')) return;

    try {
      await unregisterProduct({ serialNumber, type: 0 });
      success('Reseller removed. You can now reassign or delete this product.');
      fetchProducts();
    } catch (err) {
      console.error('Error removing reseller:', err);
      error(err.response?.data?.message || err.message || 'Failed to remove reseller');
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
            <label className="flex items-center gap-2 text-sm font-medium">
              <input
                type="checkbox"
                checked={useBulk}
                onChange={(e) => setUseBulk(e.target.checked)}
              />
              Bulk mode
            </label>
            {useBulk ? (
              <div>
                <label className="block mb-1 font-medium">Bulk Products</label>
                <textarea
                  value={bulkInput}
                  onChange={(e) => setBulkInput(e.target.value)}
                  className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:border-blue-500"
                  rows="6"
                  placeholder={'One per line: SERIAL_NUMBER,MODEL\nExample: ABC-123,Toaster X200'}
                  required
                />
              </div>
            ) : (
              <>
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
              </>
            )}
            <div className="flex gap-3">
              <button
                type="submit"
                className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
              >
                {useBulk ? 'Create Products (Bulk)' : 'Create Product'}
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
                      <span className="text-green-700 font-medium">{product.reseller?.companyName || 'Assigned'}</span>
                    ) : (
                      <span className="text-gray-400">Not assigned</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    {product.ownerId ? (
                      <span className="text-blue-600">{product.owner?.name || 'Registered'}</span>
                    ) : (
                      <span className="text-gray-400">Not owned</span>
                    )}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                    {new Date(product.registeredAt).toLocaleDateString()}
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="flex items-center gap-3">
                      {product.resellerId && !product.ownerId && (
                        <button
                          onClick={() => handleRemoveReseller(product.serialNumber)}
                          className="text-yellow-600 hover:text-yellow-800"
                          title="Remove reseller from this product"
                        >
                          <FiUserMinus />
                        </button>
                      )}
                      <button
                        onClick={() => handleDelete(product.serialNumber)}
                        className={`text-red-600 hover:text-red-800 ${product.resellerId || product.ownerId ? 'opacity-40 cursor-not-allowed hover:text-red-600' : ''}`}
                        title={product.resellerId || product.ownerId ? 'Remove reseller and consumer before deleting' : 'Delete product'}
                        disabled={Boolean(product.resellerId || product.ownerId)}
                      >
                        <FiTrash2 />
                      </button>
                    </div>
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
