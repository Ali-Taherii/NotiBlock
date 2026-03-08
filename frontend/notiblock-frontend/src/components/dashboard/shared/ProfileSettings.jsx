import { useState, useEffect, useCallback, useMemo } from 'react';
import { FiSave, FiRefreshCcw } from 'react-icons/fi';
import authService from '../../../api/auth';
import { useAuth } from '../../../hooks/useAuth';
import { useToast } from '../../../hooks/useToast';
import Toast from '../../shared/Toast';

const roleNameLabel = {
  consumer: 'Full Name',
  reseller: 'Company Name',
  manufacturer: 'Company Name',
  regulator: 'Agency Name',
};

const createFallbackAvatar = (name = '', email = '') => {
  const seed = name || email || 'User';
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(seed)}&background=4338CA&color=fff`;
};

const resolveValue = (source, keys, fallback = '') => {
  if (!source) return fallback;
  for (const key of keys) {
    if (source[key] !== undefined && source[key] !== null) {
      return source[key];
    }
    const pascalKey = key.charAt(0).toUpperCase() + key.slice(1);
    if (source[pascalKey] !== undefined && source[pascalKey] !== null) {
      return source[pascalKey];
    }
  }
  return fallback;
};

const mapProfileToForm = (profile) => ({
  name: resolveValue(profile, ['name', 'companyName', 'agencyName']),
  email: resolveValue(profile, ['email']),
  phoneNumber: resolveValue(profile, ['phoneNumber']),
  walletAddress: resolveValue(profile, ['walletAddress']),
  avatarUrl: resolveValue(profile, ['avatarUrl']),
});

export default function ProfileSettings() {
  const { user, refreshUser } = useAuth();
  const { toast, success, error, hideToast } = useToast();

  const [profile, setProfile] = useState(null);
  const [formData, setFormData] = useState(mapProfileToForm(null));
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const nameLabel = useMemo(() => roleNameLabel[user?.role] || 'Name', [user?.role]);
  const showPhoneField = user?.role === 'consumer';

  const avatarPreview = useMemo(() => {
    if (formData.avatarUrl?.trim()) {
      return formData.avatarUrl.trim();
    }

    const existingAvatar = resolveValue(profile, ['avatarUrl'], null);
    if (existingAvatar) {
      return existingAvatar;
    }

    const displayName = resolveValue(profile, ['name', 'companyName', 'agencyName']);
    const email = resolveValue(profile, ['email'], user?.email);
    return createFallbackAvatar(displayName, email);
  }, [formData.avatarUrl, profile, user?.email]);

  const fetchProfile = useCallback(async () => {
    try {
      setLoading(true);
      const data = await authService.getProfile();
      setProfile(data);
      setFormData(mapProfileToForm(data));
    } catch (err) {
      console.error('Error loading profile', err);
      error(err?.message || 'Failed to load profile');
    } finally {
      setLoading(false);
    }
  }, [error]);

  useEffect(() => {
    fetchProfile();
  }, [fetchProfile]);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleReset = () => {
    setFormData(mapProfileToForm(profile));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      const updated = await authService.updateProfile(formData);
      setProfile(updated);
      setFormData(mapProfileToForm(updated));
      await refreshUser();
      success('Profile updated successfully');
    } catch (err) {
      console.error('Profile update failed', err);
      error(err?.response?.data?.message || err?.message || 'Failed to update profile');
    } finally {
      setSaving(false);
    }
  };

  if (!user) {
    return (
      <div className="text-center py-12 text-gray-500">
        Please sign in to manage your profile.
      </div>
    );
  }

  if (loading) {
    return (
      <div className="flex items-center justify-center py-16 text-gray-500">
        Loading profile...
      </div>
    );
  }

  return (
    <div className="max-w-3xl mx-auto">
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      <form onSubmit={handleSubmit} className="space-y-8">
        <div className="flex flex-col sm:flex-row items-center gap-6 p-6 bg-indigo-50 rounded-xl border border-indigo-100">
          <img
            src={avatarPreview}
            alt="Profile avatar"
            className="w-28 h-28 rounded-full object-cover border-4 border-white shadow-lg"
          />
          <div className="flex-1 w-full">
            <label className="text-sm font-semibold text-gray-600">Avatar URL</label>
            <input
              type="url"
              name="avatarUrl"
              value={formData.avatarUrl}
              onChange={handleChange}
              placeholder="https://example.com/avatar.png"
              className="mt-2 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:outline-none"
            />
            <p className="text-xs text-gray-500 mt-2">
              Paste an image URL or leave blank to use the generated avatar.
            </p>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
          <div>
            <label className="text-sm font-semibold text-gray-600">{nameLabel}</label>
            <input
              type="text"
              name="name"
              value={formData.name}
              onChange={handleChange}
              className="mt-2 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:outline-none"
              placeholder={nameLabel}
              required
            />
          </div>

          <div>
            <label className="text-sm font-semibold text-gray-600">Email Address</label>
            <input
              type="email"
              name="email"
              value={formData.email}
              onChange={handleChange}
              className="mt-2 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:outline-none"
              placeholder="you@example.com"
              required
            />
          </div>

          {showPhoneField && (
            <div>
              <label className="text-sm font-semibold text-gray-600">Phone Number</label>
              <input
                type="tel"
                name="phoneNumber"
                value={formData.phoneNumber}
                onChange={handleChange}
                className="mt-2 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:outline-none"
                placeholder="+1 (555) 123-4567"
              />
            </div>
          )}

          <div>
            <label className="text-sm font-semibold text-gray-600">Wallet Address</label>
            <input
              type="text"
              name="walletAddress"
              value={formData.walletAddress}
              onChange={handleChange}
              className="mt-2 w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-indigo-500 focus:outline-none font-mono"
              placeholder="0x..."
            />
          </div>
        </div>

        <div className="flex flex-wrap items-center gap-3 justify-between">
          <div className="text-sm text-gray-500">
            Last updated: {new Date(resolveValue(profile, ['lastUpdatedAt'], resolveValue(profile, ['createdAt'], Date.now()))).toLocaleString()}
          </div>
          <div className="flex gap-3">
            <button
              type="button"
              onClick={handleReset}
              className="inline-flex items-center gap-2 px-4 py-2 rounded-lg border border-gray-300 text-gray-700 hover:bg-gray-50"
              disabled={saving}
            >
              <FiRefreshCcw /> Reset
            </button>
            <button
              type="submit"
              className="inline-flex items-center gap-2 px-5 py-2 rounded-lg bg-indigo-600 text-white hover:bg-indigo-700 disabled:opacity-50"
              disabled={saving}
            >
              <FiSave /> {saving ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
}
