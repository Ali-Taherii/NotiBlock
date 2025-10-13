import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "../pages/Home";
import Recalls from "../pages/Recalls";
import AuthPage from "../pages/Auth";
import ManufacturerDashboard from "../pages/ManufacturerDashboard";
import ConsumerDashboard from "../pages/ConsumerDashboard";
import RegulatorDashboard from "../pages/RegulatorDashboard";
import ReportIssue from "../components/dashboard/Consumer/ReportIssue";
import MyTickets from "../components/dashboard/Consumer/MyTickets";
import { ProtectedRoute } from "./ProtectedRoute";
import NotFound from "../pages/NotFound";

export default function AppRouter() {

  return (
    <Router>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/not-found" element={<NotFound />} />
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

        {/* Consumer specific routes */}
        <Route path="/consumer/report-issue" element={<ReportIssue />} />
        <Route path="/consumer/my-tickets" element={
          <ProtectedRoute role="consumer">
            <MyTickets />
          </ProtectedRoute>
        } />
      </Routes>
    </Router>
  );
}
