import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "../pages/Home";
import Recalls from "../pages/Recalls";
import AuthPage from "../pages/Auth";
import Notifications from "../pages/Notifications";
import ManufacturerDashboard from "../pages/ManufacturerDashboard";
import ConsumerDashboard from "../pages/ConsumerDashboard";
import RegulatorDashboard from "../pages/RegulatorDashboard";
import ResellerDashboard from "../pages/ResellerDashboard";
import { ProtectedRoute } from "./ProtectedRoute";

export default function AppRouter() {

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/recalls" element={<Recalls />} />
        <Route path="/auth" element={<AuthPage />} />
        
        {/* Notifications - accessible to all authenticated users */}
        <Route path="/notifications" element={
          <ProtectedRoute>
            <Notifications />
          </ProtectedRoute>
        } />

        {/* Dashboard routes */}
        <Route path="/manufacturer/dashboard" element={
          <ProtectedRoute role="manufacturer">
            <ManufacturerDashboard />
          </ProtectedRoute>
        } />
        <Route path="/consumer/dashboard" element={
          <ProtectedRoute role="consumer">
            <ConsumerDashboard />
          </ProtectedRoute>
        } />
        <Route path="/regulator/dashboard" element={
          <ProtectedRoute role="regulator">
            <RegulatorDashboard />
          </ProtectedRoute>
        } />
        <Route path="/reseller/dashboard" element={
          <ProtectedRoute role="reseller">
            <ResellerDashboard />
          </ProtectedRoute>
        } />

        {/* other routes as needed */}
      </Routes>
    </Router>
  );
}
