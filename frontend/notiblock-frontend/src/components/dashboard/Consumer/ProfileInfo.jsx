import { useAuth } from '../../../hooks/useAuth';

const ProfileInfo = () => {
    const { user } = useAuth();

    if (!user) {
        return (
            <div className="flex items-center justify-center h-64">
                <span className="text-gray-500 text-lg">Loading profile...</span>
            </div>
        );
    }

    return (
        <form className="max-w-md mx-auto bg-white rounded-xl shadow-md overflow-hidden mt-8 p-6 flex flex-col gap-6">
            <div className="flex items-center gap-6">
                <img
                    className="h-20 w-20 rounded-full object-cover border-2 border-indigo-500"
                    src={user.avatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(user.name || 'AT')}`}
                    alt="User avatar"
                />
                <div className='flex flex-col'>
                    <h2 className="text-2xl font-bold text-gray-800">{user.name || 'Ali Taheri'}</h2>
                    {user.role && (
                        <span className="inline-block mt-2 px-3 py-1 text-xs font-semibold bg-indigo-100 text-indigo-700 rounded-full">
                            {user.role}
                        </span>
                    )}
                </div>
            </div>
            <div className="flex flex-col gap-4">
                <div className="flex items-center">
                    <label className="w-24 text-gray-600" htmlFor="name">Name</label>
                    <input
                        id="name"
                        type="text"
                        value={user.name || 'Ali Taheri'}
                        disabled
                        className="flex-1 px-3 py-2 border rounded bg-gray-100 text-gray-800"
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
                {user.phone && (
                    <div className="flex items-center">
                        <label className="w-24 text-gray-600" htmlFor="phone">Phone</label>
                        <input
                            id="phone"
                            type="text"
                            value={user.phone}
                            disabled
                            className="flex-1 px-3 py-2 border rounded bg-gray-100 text-gray-800"
                        />
                    </div>
                )}
                {user.address && (
                    <div className="flex items-center">
                        <label className="w-24 text-gray-600" htmlFor="address">Address</label>
                        <input
                            id="address"
                            type="text"
                            value={user.address}
                            disabled
                            className="flex-1 px-3 py-2 border rounded bg-gray-100 text-gray-800"
                        />
                    </div>
                )}
            </div>
        </form>
    );
};

export default ProfileInfo;