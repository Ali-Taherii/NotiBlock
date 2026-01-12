import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "../pages/Home";
import Recalls from "../pages/Recalls";
import AuthPage from "../pages/Auth";
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
        {/* <Route path="/not-found" element={<NotFound />} /> */}
        <Route path="/recalls" element={<Recalls />} />
        <Route path="/auth" element={<AuthPage />} />

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

        {/* Consumer specific routes */}
        {/* <Route path="/consumer/report-issue" element={
          <ProtectedRoute role="consumer">
            <ReportIssue />
          </ProtectedRoute>
        } />
        <Route path="/consumer/my-tickets" element={
          <ProtectedRoute role="consumer">
            <MyTickets />
          </ProtectedRoute>
        } />
        <Route path="/consumer/profile-info" element={
          <ProtectedRoute role="consumer">
            <ProfileInfo />
          </ProtectedRoute>
        } /> */}

        {/* Catch-all route for 404 */}
        {/* <Route path="*" element={<NotFound />} /> */}
      </Routes>
    </Router>
  );
}
