import { useAuth } from '../../../hooks/useAuth';

const ProfileInfo = () => {
    const { user, loading } = useAuth();

    if (loading) {
        return (
            <div className="flex items-center justify-center h-64">
                <span className="text-gray-500 text-lg">Loading profile...</span>
            </div>
        );
    }

    if (!user) {
        return (
            <div className="flex items-center justify-center h-64">
                <span className="text-gray-500 text-lg">No user data available</span>
            </div>
        );
    }

    return (
        <form className="max-w-md mx-auto bg-white rounded-xl shadow-md overflow-hidden mt-8 p-6 flex flex-col gap-6">
            <div className="flex items-center gap-6">
                <img
                    className="h-20 w-20 rounded-full object-cover border-2 border-indigo-500"
                    src={`https://ui-avatars.com/api/?name=${encodeURIComponent(user.email || 'User')}&background=4F46E5&color=fff`}
                    alt="User avatar"
                />
                <div className='flex flex-col'>
                    <h2 className="text-2xl font-bold text-gray-800">{user.email}</h2>
                    {user.role && (
                        <span className="inline-block mt-2 px-3 py-1 text-xs font-semibold bg-indigo-100 text-indigo-700 rounded-full uppercase">
                            {user.role}
                        </span>
                    )}
                </div>
            </div>
            <div className="flex flex-col gap-4">
                <div className="flex items-center">
                    <label className="w-24 text-gray-600" htmlFor="userId">User ID</label>
                    <input
                        id="userId"
                        type="text"
                        value={user.userId || 'N/A'}
                        disabled
                        className="flex-1 px-3 py-2 border rounded bg-gray-100 text-gray-800 font-mono text-sm"
                    />
                </div>
                <div className="flex items-center">
                    <label className="w-24 text-gray-600" htmlFor="email">Email</label>
                    <input
                        id="email"
                        type="email"
                        value={user.email || ''}
                        disabled
                        className="flex-1 px-3 py-2 border rounded bg-gray-100 text-gray-800"
                    />
                </div>
                <div className="flex items-center">
                    <label className="w-24 text-gray-600" htmlFor="role">Role</label>
                    <input
                        id="role"
                        type="text"
                        value={user.role || 'N/A'}
                        disabled
                        className="flex-1 px-3 py-2 border rounded bg-gray-100 text-gray-800 capitalize"
                    />
                </div>
            </div>
        </form>
    );
};

export default ProfileInfo;