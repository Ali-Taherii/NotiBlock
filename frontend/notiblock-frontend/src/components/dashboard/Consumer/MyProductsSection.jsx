import { useState, useEffect, useCallback } from 'react';
import { FiPlus, FiTrash2, FiPackage } from 'react-icons/fi';
import { getMyProducts, registerProduct, unregisterProduct } from '../../../api/products';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

export default function MyProductsSection() {
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showRegisterForm, setShowRegisterForm] = useState(false);
  const [formData, setFormData] = useState({ serialNumber: '' });
  const { toast, success, error, hideToast } = useToast();

  const fetchProducts = useCallback(async () => {
    try {
      setLoading(true);
      const response = await getMyProducts('consumer');
      const items = response?.data?.items || response?.items || response || [];
      setProducts(items);
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

  const handleRegister = async (e) => {
    e.preventDefault();
    try {
      await registerProduct(formData);
      success('Product registered successfully!');
      setFormData({ serialNumber: ''});
      setShowRegisterForm(false);
      fetchProducts();
    } catch (err) {
      console.error('Error registering product:', err);
      error(err.response?.data?.message || 'Failed to register product');
    }
  };

  const handleUnregister = async (serialNumber) => {
    if (!window.confirm('Are you sure you want to unregister this product?')) return;
    
    try {
      await unregisterProduct({ serialNumber, type: 2 });
      success('Product unregistered successfully!');
      fetchProducts();
    } catch (err) {
      console.error('Error unregistering product:', err);
      error(err.response?.data?.message || 'Failed to unregister product');
    }
  };

  if (loading) {
    return <div className="text-center py-8">Loading products...</div>;
  }

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <div className="flex justify-between items-center mb-6">
        <h2 className="text-xl font-semibold text-gray-800">My Registered Products</h2>
        <button
          onClick={() => setShowRegisterForm(!showRegisterForm)}
          className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          <FiPlus className="w-4 h-4" />
          {showRegisterForm ? 'Cancel' : 'Register Product'}
        </button>
      </div>

      {/* Register Form */}
      {showRegisterForm && (
        <div className="bg-gray-50 p-6 rounded-lg mb-6 border border-gray-200">
          <h3 className="text-lg font-semibold mb-4">Register New Product</h3>
          <form onSubmit={handleRegister} className="space-y-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Product Serial Number
              </label>
              <input
                type="text"
                value={formData.serialNumber}
                onChange={(e) => setFormData({ ...formData, serialNumber: e.target.value })}
                className="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500"
                required
              />
            </div>
            <button
              type="submit"
              className="w-full px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
            >
              Register Product
            </button>
          </form>
        </div>
      )}

      {/* Products Table */}
      {products.length === 0 ? (
        <div className="text-center py-12 bg-gray-50 rounded-lg">
          <FiPackage className="w-12 h-12 text-gray-400 mx-auto mb-3" />
          <p className="text-gray-600">No registered products yet</p>
          <p className="text-sm text-gray-500 mt-1">Click "Register Product" to add your first product</p>
        </div>
      ) : (
        <div className="overflow-x-auto">
          <table className="w-full">
            <thead>
              <tr className="border-b border-gray-200">
                <th className="text-left py-3 px-4 font-medium text-gray-700">Serial Number</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Model</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Manufacturer</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Owner Name</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Owner Email</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Registered Date</th>
                <th className="text-left py-3 px-4 font-medium text-gray-700">Actions</th>
              </tr>
            </thead>
            <tbody>
              {products.map((product) => (
                <tr key={product.serialNumber} className="border-b border-gray-100 hover:bg-gray-50">
                  <td className="py-3 px-4 font-mono text-sm">{product.serialNumber}</td>
                  <td className="py-3 px-4">{product.model}</td>
                  <td className="py-3 px-4">{product.manufacturer.companyName || 'N/A'}</td>
                  <td className="py-3 px-4">{product.owner.name || 'N/A'}</td>
                  <td className="py-3 px-4">{product.owner.email || 'N/A'}</td>
                  <td className="py-3 px-4">
                    {product.registeredAt ? new Date(product.registeredAt).toLocaleDateString() : 'N/A'}
                  </td>
                  <td className="py-3 px-4">
                    <button
                      onClick={() => handleUnregister(product.serialNumber)}
                      className="text-red-600 hover:text-red-800 transition-colors"
                      title="Unregister Product"
                    >
                      <FiTrash2 className="w-4 h-4" />
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
