import { useEffect, useState } from "react";
import { AuthContext } from "./AuthContext";
import { apiFetch } from "../api/api"; 

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  const signup = async (...args) => {
    // Handle both old signature (email, password, phoneNumber, name, walletAddress, role) 
    // and new simplified signature (email, password, role)
    let email, password, actualRole, actualPhoneNumber, actualName, actualWalletAddress;
    
    if (args.length === 3) {
      // New simplified signature: (email, password, role)
      [email, password, actualRole] = args;
      actualPhoneNumber = "";
      actualName = "";
      actualWalletAddress = "";
    } else {
      // Old signature: (email, password, phoneNumber, name, walletAddress, role)
      [email, password, actualPhoneNumber, actualName, actualWalletAddress, actualRole] = args;
    }

    await apiFetch(`/auth/${actualRole}/register`, {
      method: "POST",
      body: JSON.stringify({ 
        email, 
        password, 
        phoneNumber: actualPhoneNumber, 
        name: actualName, 
        walletAddress: actualWalletAddress 
      }),
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

  const login = async (email, password, role) => {
    await apiFetch(`/auth/${role}/login`, {
      method: "POST",
      body: JSON.stringify({ email, password}),
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
