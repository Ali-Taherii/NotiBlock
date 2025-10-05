import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "../pages/Home";
import Recalls from "../pages/Recalls";
import AuthPage from "../pages/Auth";

export default function AppRouter() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/recalls" element={<Recalls />} />
        <Route path="/auth" element={<AuthPage />} />
      </Routes>
    </Router>
  );
}
