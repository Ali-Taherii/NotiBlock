import { Link } from "react-router-dom";
// eslint-disable-next-line no-unused-vars
import { motion } from "framer-motion";

export default function NotFound() {
  return (
    <div className="flex flex-col items-center justify-center min-h-screen bg-gradient-to-b from-indigo-50 to-white text-center px-6">
      <motion.div
        initial={{ y: -20, opacity: 0 }}
        animate={{ y: 0, opacity: 1 }}
        transition={{ duration: 0.6 }}
        className="flex flex-col items-center"
      >
        <motion.span
          initial={{ rotate: -10 }}
          animate={{ rotate: 10 }}
          transition={{
            repeat: Infinity,
            repeatType: "reverse",
            duration: 1.5,
          }}
          className="text-8xl mb-4"
        >
          🐣
        </motion.span>
        <h1 className="text-5xl font-bold text-indigo-600 mb-2">Oops!</h1>
        <p className="text-gray-600 mb-6 text-lg">
          The page you’re looking for wandered off...
        </p>

        <Link
          to="/auth"
          className="px-6 py-3 rounded-2xl bg-indigo-500 hover:bg-indigo-600 text-white shadow-lg transition-all"
        >
          Take me home 🏠
        </Link>

        <p className="mt-8 text-sm text-gray-400">
          Error code: <span className="font-mono">404</span>
        </p>
      </motion.div>
    </div>
  );
}
