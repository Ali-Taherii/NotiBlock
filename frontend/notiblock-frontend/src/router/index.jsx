import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Home from "../pages/Home";
import Recalls from "../pages/Recalls";

export default function AppRouter() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/recalls" element={<Recalls />} />
      </Routes>
    </Router>
  );
}
