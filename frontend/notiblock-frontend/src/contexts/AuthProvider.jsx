import { useEffect, useState } from "react";
import { AuthContext } from "./AuthContext";
import { apiFetch } from "../api/api"; 

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const signup = async (email, password, role) => {
    await apiFetch("/auth/register", {
      method: "POST",
      body: JSON.stringify({ email, password, role }),
    });
    const fetchedUser = await fetchUser();
    return fetchedUser;
  };

  const fetchUser = async () => {
    try {
      const data = await apiFetch("/auth/me");
      setUser(data);
      return data;
    } catch {
      setUser(null);
    } finally {
      setLoading(false);
    }
  };

  const login = async (email, password) => {
    await apiFetch("/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
    const fetchedUser = await fetchUser();
    return fetchedUser;
  };

  const logout = async () => {
    await apiFetch("/auth/logout", { method: "POST" });
    setUser(null);
  };

  useEffect(() => {
    fetchUser(); // auto-fetch on mount
  }, []);

  return (
    <AuthContext.Provider value={{ user, signup, login, logout, loading, fetchUser }}>
      {children}
    </AuthContext.Provider>
  );
};
