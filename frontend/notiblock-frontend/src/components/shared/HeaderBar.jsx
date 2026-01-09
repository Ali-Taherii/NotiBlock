import { FaBell } from "react-icons/fa";
import { useAuth } from "../../hooks/useAuth";

export default function HeaderBar() {

    const { user } = useAuth();
    const isLoggedIn = !!user;

    return (
        <header className="bg-gradient-to-r from-indigo-700 via-purple-700 to-pink-600 px-8 py-4 text-white flex items-center shadow-lg relative">
            <h1 className="m-0 text-3xl font-extrabold tracking-widest drop-shadow-lg">NotiBlock</h1>
            <div className="ml-auto flex items-center space-x-4">
                {!isLoggedIn ? (
                    <a
                        href="/auth"
                        className="bg-white text-indigo-700 font-semibold px-4 py-2 rounded shadow hover:bg-gray-100 transition"
                    >
                        Login
                    </a>
                ) : (
                    <div className="flex items-center space-x-4">
                        <a
                            href="/consumer/profile-info"
                            className="bg-white text-indigo-700 font-semibold px-4 py-2 rounded shadow hover:bg-gray-100 transition"
                        >
                            My Profile
                        </a>
                        <img
                            src="https://i.pravatar.cc/40?img=3"
                            alt="User Avatar"
                            className="w-10 h-10 rounded-full border-2 border-white shadow z-0"
                        />
                    </div>
                )}

            </div>
        </header>
    );
}
