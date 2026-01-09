import { useState } from 'react';
import { Link } from 'react-router-dom';
import { 
  FiShield, FiBell, FiSmartphone, FiUsers, FiBarChart2, FiGlobe,
  FiCheckCircle, FiClock, FiAlertTriangle, FiTrendingUp, FiPackage,
  FiChevronDown, FiChevronUp
} from 'react-icons/fi';

export default function Home() {
  const [activeUserType, setActiveUserType] = useState('consumer');
  const [expandedFaq, setExpandedFaq] = useState(null);

  const features = [
    {
      icon: FiShield,
      title: 'Blockchain Security',
      description: 'Immutable recall records on distributed ledger ensure data integrity and transparency.',
    },
    {
      icon: FiBell,
      title: 'Instant Notifications',
      description: 'Real-time alerts via email and dashboard when products you own are recalled.',
    },
    {
      icon: FiSmartphone,
      title: 'QR Code Integration',
      description: 'Scan product QR codes to register instantly and access product information.',
    },
    {
      icon: FiUsers,
      title: 'Multi-Stakeholder Platform',
      description: 'Connects consumers, manufacturers, resellers, and regulators in one network.',
    },
    {
      icon: FiBarChart2,
      title: 'Transparency Dashboard',
      description: 'Track your products, view recall history, and access detailed analytics.',
    },
    {
      icon: FiGlobe,
      title: 'Global Compliance',
      description: 'Meets regulatory requirements and standards worldwide for product safety.',
    },
  ];

  const userTypes = {
    consumer: {
      title: 'For Consumers',
      benefits: [
        'Register products you own',
        'Get instant recall alerts',
        'Submit safety reports',
        'View product history',
      ],
      cta: 'Sign Up as Consumer',
      icon: FiPackage,
    },
    manufacturer: {
      title: 'For Manufacturers',
      benefits: [
        'Create product batches',
        'Issue recalls instantly',
        'Track notification reach',
        'Manage product lifecycle',
      ],
      cta: 'Sign Up as Manufacturer',
      icon: FiTrendingUp,
    },
    reseller: {
      title: 'For Resellers',
      benefits: [
        'Track inventory recalls',
        'Submit consumer reports',
        'View approved tickets',
        'Manage product sales',
      ],
      cta: 'Sign Up as Reseller',
      icon: FiBarChart2,
    },
    regulator: {
      title: 'For Regulators',
      benefits: [
        'Monitor all recalls',
        'Approve manufacturer actions',
        'Access compliance reports',
        'Ensure public safety',
      ],
      cta: 'Sign Up as Regulator',
      icon: FiShield,
    },
  };

  const faqs = [
    {
      question: 'How do I register my products?',
      answer: 'Simply create a consumer account, navigate to your dashboard, and scan the QR code on your product or enter the serial number manually. You\'ll receive instant confirmation and automatic recall notifications.',
    },
    {
      question: 'Is NotiBlock free for consumers?',
      answer: 'Yes! NotiBlock is completely free for consumers. We believe everyone deserves to stay safe and informed about the products they own.',
    },
    {
      question: 'How secure is my data?',
      answer: 'We use blockchain technology to ensure data integrity and security. Your personal information is encrypted and stored securely. We never share your data with third parties without your consent.',
    },
    {
      question: 'What happens when a recall is issued?',
      answer: 'You\'ll receive instant notifications through email and your dashboard. The notification includes recall details, severity level, reason for recall, and recommended actions to take.',
    },
    {
      question: 'Can I use NotiBlock for business purposes?',
      answer: 'Absolutely! We offer specialized accounts for manufacturers, resellers, and regulators with additional features for product management, recall issuance, and compliance tracking.',
    },
  ];

  const recentRecalls = [
    { product: 'SmartHome Toaster X200', reason: 'Fire hazard', affected: 234 },
    { product: 'TurboPro Fan Series', reason: 'Blade detachment risk', affected: 567 },
    { product: 'SafeGuard Baby Monitor', reason: 'Security vulnerability', affected: 123 },
  ];

  return (
    <div className="min-h-screen bg-white">
      {/* Hero Section */}
      <section className="bg-gradient-to-br from-blue-600 via-purple-600 to-pink-500 text-white py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center">
            <h1 className="text-5xl md:text-6xl font-extrabold mb-6">
              Stay Safe. Stay Informed.
            </h1>
            <p className="text-xl md:text-2xl mb-8 text-blue-100">
              Never miss a product recall again. Powered by blockchain technology.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link
                to="/auth"
                className="bg-white text-blue-600 px-8 py-3 rounded-lg font-semibold text-lg hover:bg-gray-100 transition-colors shadow-lg"
              >
                Get Started Free
              </Link>
              <a
                href="#features"
                className="border-2 border-white text-white px-8 py-3 rounded-lg font-semibold text-lg hover:bg-white hover:text-blue-600 transition-colors"
              >
                Learn More
              </a>
            </div>
          </div>
        </div>
      </section>

      {/* Problem Statement */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12 text-gray-800">
            The Problem with Traditional Recalls
          </h2>
          <div className="grid md:grid-cols-3 gap-8">
            <div className="text-center">
              <div className="text-red-500 mb-4">
                <FiAlertTriangle className="w-16 h-16 mx-auto" />
              </div>
              <h3 className="text-xl font-semibold mb-2 text-gray-800">70% Never Reached</h3>
              <p className="text-gray-600">
                Most recalled products never reach consumers through traditional methods
              </p>
            </div>
            <div className="text-center">
              <div className="text-orange-500 mb-4">
                <FiClock className="w-16 h-16 mx-auto" />
              </div>
              <h3 className="text-xl font-semibold mb-2 text-gray-800">23 Days Average</h3>
              <p className="text-gray-600">
                Traditional recalls take weeks to notify product owners
              </p>
            </div>
            <div className="text-center">
              <div className="text-yellow-500 mb-4">
                <FiPackage className="w-16 h-16 mx-auto" />
              </div>
              <h3 className="text-xl font-semibold mb-2 text-gray-800">Manual Tracking</h3>
              <p className="text-gray-600">
                Time-consuming and unreliable manual product registration
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* How It Works */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12 text-gray-800">
            How NotiBlock Works
          </h2>
          <div className="grid md:grid-cols-3 gap-12">
            <div className="text-center">
              <div className="bg-blue-100 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4">
                <span className="text-2xl font-bold text-blue-600">1</span>
              </div>
              <h3 className="text-xl font-semibold mb-2 text-gray-800">Register Products</h3>
              <p className="text-gray-600">
                Scan QR code or enter serial number to register your products instantly
              </p>
            </div>
            <div className="text-center">
              <div className="bg-purple-100 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4">
                <span className="text-2xl font-bold text-purple-600">2</span>
              </div>
              <h3 className="text-xl font-semibold mb-2 text-gray-800">Get Notified</h3>
              <p className="text-gray-600">
                Receive instant alerts when your products are recalled
              </p>
            </div>
            <div className="text-center">
              <div className="bg-pink-100 w-16 h-16 rounded-full flex items-center justify-center mx-auto mb-4">
                <span className="text-2xl font-bold text-pink-600">3</span>
              </div>
              <h3 className="text-xl font-semibold mb-2 text-gray-800">Take Action</h3>
              <p className="text-gray-600">
                Follow recommended steps to ensure your safety immediately
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Key Features */}
      <section id="features" className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12 text-gray-800">
            Powerful Features for Product Safety
          </h2>
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8">
            {features.map((feature, index) => {
              const Icon = feature.icon;
              return (
                <div
                  key={index}
                  className="bg-white p-6 rounded-lg shadow-md hover:shadow-xl transition-shadow"
                >
                  <div className="text-blue-600 mb-4">
                    <Icon className="w-12 h-12" />
                  </div>
                  <h3 className="text-xl font-semibold mb-2 text-gray-800">
                    {feature.title}
                  </h3>
                  <p className="text-gray-600">{feature.description}</p>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* User Type Selector */}
      <section className="py-16 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12 text-gray-800">
            Choose Your Account Type
          </h2>
          
          {/* Tab Buttons */}
          <div className="flex flex-wrap justify-center gap-4 mb-8">
            {Object.keys(userTypes).map((type) => {
              const Icon = userTypes[type].icon;
              return (
                <button
                  key={type}
                  onClick={() => setActiveUserType(type)}
                  className={`flex items-center gap-2 px-6 py-3 rounded-lg font-semibold transition-all ${
                    activeUserType === type
                      ? 'bg-blue-600 text-white shadow-lg'
                      : 'bg-gray-100 text-gray-700 hover:bg-gray-200'
                  }`}
                >
                  <Icon className="w-5 h-5" />
                  {type.charAt(0).toUpperCase() + type.slice(1)}
                </button>
              );
            })}
          </div>

          {/* Tab Content */}
          <div className="bg-gray-50 rounded-lg p-8 max-w-2xl mx-auto">
            <h3 className="text-2xl font-bold mb-6 text-gray-800">
              {userTypes[activeUserType].title}
            </h3>
            <ul className="space-y-3 mb-6">
              {userTypes[activeUserType].benefits.map((benefit, index) => (
                <li key={index} className="flex items-start gap-3">
                  <FiCheckCircle className="w-6 h-6 text-green-500 flex-shrink-0 mt-0.5" />
                  <span className="text-gray-700">{benefit}</span>
                </li>
              ))}
            </ul>
            <Link
              to="/auth"
              className="block w-full text-center bg-blue-600 text-white px-6 py-3 rounded-lg font-semibold hover:bg-blue-700 transition-colors"
            >
              {userTypes[activeUserType].cta}
            </Link>
          </div>
        </div>
      </section>

      {/* Statistics */}
      <section className="py-16 bg-gradient-to-r from-blue-600 to-purple-600 text-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid md:grid-cols-4 gap-8 text-center">
            <div>
              <div className="text-4xl font-bold mb-2">1.2M+</div>
              <div className="text-blue-100">Active Products</div>
            </div>
            <div>
              <div className="text-4xl font-bold mb-2">156</div>
              <div className="text-blue-100">Recalls Issued</div>
            </div>
            <div>
              <div className="text-4xl font-bold mb-2">45K+</div>
              <div className="text-blue-100">Users Protected</div>
            </div>
            <div>
              <div className="text-4xl font-bold mb-2">&lt;2min</div>
              <div className="text-blue-100">Response Time</div>
            </div>
          </div>
        </div>
      </section>

      {/* Recent Recalls */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12 text-gray-800">
            Recent Recalls
          </h2>
          <div className="space-y-4 max-w-3xl mx-auto">
            {recentRecalls.map((recall, index) => (
              <div
                key={index}
                className="bg-white p-6 rounded-lg shadow-md flex items-center justify-between"
              >
                <div className="flex items-center gap-4">
                  <FiAlertTriangle className="w-8 h-8 text-red-500" />
                  <div>
                    <h3 className="font-semibold text-gray-800">{recall.product}</h3>
                    <p className="text-sm text-gray-600">{recall.reason}</p>
                  </div>
                </div>
                <div className="text-right">
                  <div className="font-semibold text-blue-600">{recall.affected}</div>
                  <div className="text-xs text-gray-500">users notified</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Comparison Table */}
      <section className="py-16 bg-white">
        <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12 text-gray-800">
            Why Choose NotiBlock?
          </h2>
          <div className="overflow-x-auto">
            <table className="w-full">
              <thead>
                <tr className="border-b-2 border-gray-300">
                  <th className="py-4 px-6 text-left text-gray-800 font-semibold">Feature</th>
                  <th className="py-4 px-6 text-center text-gray-800 font-semibold">Traditional</th>
                  <th className="py-4 px-6 text-center text-blue-600 font-semibold">NotiBlock</th>
                </tr>
              </thead>
              <tbody>
                <tr className="border-b border-gray-200">
                  <td className="py-4 px-6 text-gray-700">Notification Speed</td>
                  <td className="py-4 px-6 text-center text-gray-500">7-30 days</td>
                  <td className="py-4 px-6 text-center text-green-600 font-semibold">&lt;2 minutes</td>
                </tr>
                <tr className="border-b border-gray-200">
                  <td className="py-4 px-6 text-gray-700">Registration Method</td>
                  <td className="py-4 px-6 text-center text-gray-500">Manual forms</td>
                  <td className="py-4 px-6 text-center text-green-600 font-semibold">QR Code scan</td>
                </tr>
                <tr className="border-b border-gray-200">
                  <td className="py-4 px-6 text-gray-700">Tracking</td>
                  <td className="py-4 px-6 text-center text-gray-500">None</td>
                  <td className="py-4 px-6 text-center text-green-600 font-semibold">Full transparency</td>
                </tr>
                <tr className="border-b border-gray-200">
                  <td className="py-4 px-6 text-gray-700">Reach</td>
                  <td className="py-4 px-6 text-center text-gray-500">30% average</td>
                  <td className="py-4 px-6 text-center text-green-600 font-semibold">95%+ reach</td>
                </tr>
                <tr className="border-b border-gray-200">
                  <td className="py-4 px-6 text-gray-700">Cost for Consumers</td>
                  <td className="py-4 px-6 text-center text-gray-500">Free</td>
                  <td className="py-4 px-6 text-center text-green-600 font-semibold">Free</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>

      {/* FAQ Section */}
      <section className="py-16 bg-gray-50">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 className="text-3xl font-bold text-center mb-12 text-gray-800">
            Frequently Asked Questions
          </h2>
          <div className="space-y-4">
            {faqs.map((faq, index) => (
              <div
                key={index}
                className="bg-white rounded-lg shadow-md overflow-hidden"
              >
                <button
                  onClick={() => setExpandedFaq(expandedFaq === index ? null : index)}
                  className="w-full px-6 py-4 text-left flex items-center justify-between hover:bg-gray-50 transition-colors"
                >
                  <span className="font-semibold text-gray-800">{faq.question}</span>
                  {expandedFaq === index ? (
                    <FiChevronUp className="w-5 h-5 text-gray-600" />
                  ) : (
                    <FiChevronDown className="w-5 h-5 text-gray-600" />
                  )}
                </button>
                {expandedFaq === index && (
                  <div className="px-6 pb-4 text-gray-600">
                    {faq.answer}
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Final CTA */}
      <section className="py-20 bg-gradient-to-r from-blue-600 to-purple-600 text-white">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-4xl font-bold mb-4">Ready to Protect Your Family?</h2>
          <p className="text-xl mb-8 text-blue-100">
            Join 45,000+ protected users today
          </p>
          <Link
            to="/auth"
            className="inline-block bg-white text-blue-600 px-10 py-4 rounded-lg font-semibold text-lg hover:bg-gray-100 transition-colors shadow-lg"
          >
            Get Started Free
          </Link>
          <p className="mt-4 text-sm text-blue-100">No credit card required</p>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-900 text-gray-300 py-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid md:grid-cols-4 gap-8">
            <div>
              <h3 className="text-white font-bold text-lg mb-4">NotiBlock</h3>
              <p className="text-sm">
                Blockchain-powered product safety network protecting families worldwide.
              </p>
            </div>
            <div>
              <h4 className="text-white font-semibold mb-4">Product</h4>
              <ul className="space-y-2 text-sm">
                <li><a href="#features" className="hover:text-white transition-colors">Features</a></li>
                <li><Link to="/recalls" className="hover:text-white transition-colors">Recalls</Link></li>
                <li><Link to="/auth" className="hover:text-white transition-colors">Get Started</Link></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-semibold mb-4">Company</h4>
              <ul className="space-y-2 text-sm">
                <li><a href="#" className="hover:text-white transition-colors">About</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Blog</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Contact</a></li>
              </ul>
            </div>
            <div>
              <h4 className="text-white font-semibold mb-4">Legal</h4>
              <ul className="space-y-2 text-sm">
                <li><a href="#" className="hover:text-white transition-colors">Privacy Policy</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Terms of Service</a></li>
                <li><a href="#" className="hover:text-white transition-colors">Cookie Policy</a></li>
              </ul>
            </div>
          </div>
          <div className="border-t border-gray-800 mt-8 pt-8 text-center text-sm">
            <p>&copy; 2026 NotiBlock. All rights reserved.</p>
          </div>
        </div>
      </footer>
    </div>
  );
}
