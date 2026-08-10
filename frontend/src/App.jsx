import { lazy, Suspense } from 'react'
import { Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import Footer from './components/Footer'
import ProtectedRoute from './components/ProtectedRoute'
import Home from './pages/guest/Home'
import JobDetail from './pages/guest/JobDetail'
import CompanyDetail from './pages/guest/CompanyDetail'
import Login from './pages/auth/Login'
import Register from './pages/auth/Register'
import VerifyEmail from './pages/auth/VerifyEmail'
import ForgotPassword from './pages/auth/ForgotPassword'
import ResetPassword from './pages/auth/ResetPassword'
import PostJob from './pages/employer/PostJob'
import Applicants from './pages/employer/Applicants'
import CompanyProfile from './pages/employer/CompanyProfile'
import MyJobs from './pages/employer/MyJobs'
import PackagePlans from './pages/employer/PackagePlans'
import ApproveJobs from './pages/admin/ApproveJobs'
import AdminLayout from './pages/admin/AdminLayout'
import RemovedJobs from './pages/admin/RemovedJobs'
import AdminAccounts from './pages/admin/AdminAccounts'
import AdminSkills from './pages/admin/AdminSkills'
import AdminIndustries from './pages/admin/AdminIndustries'
const AdminReports = lazy(() => import('./pages/admin/AdminReports'))
import PackageManagement from './pages/admin/PackageManagement'
import ManageCv from './pages/candidate/ManageCv'
import Profile from './pages/candidate/Profile'
import MyApplications from './pages/candidate/MyApplications'
import Favorites from './pages/candidate/Favorites'
import './App.css'

function App() {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', minHeight: '100vh' }}>
      <Header />
      <div style={{ flex: 1 }}>
      <Suspense fallback={null}>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/jobs/:id" element={<JobDetail />} />
        <Route path="/companies/:id" element={<CompanyDetail />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/verify-email" element={<VerifyEmail />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />
        <Route path="/reset-password" element={<ResetPassword />} />
        <Route path="/employer/post-job" element={
          <ProtectedRoute role="NhaTuyenDung"><PostJob /></ProtectedRoute>
        } />
        <Route path="/employer/jobs/:id/edit" element={
          <ProtectedRoute role="NhaTuyenDung"><PostJob /></ProtectedRoute>
        } />
        <Route path="/employer/jobs/:id/applicants" element={
          <ProtectedRoute role="NhaTuyenDung"><Applicants /></ProtectedRoute>
        } />
        <Route path="/employer/my-jobs" element={
          <ProtectedRoute role="NhaTuyenDung"><MyJobs /></ProtectedRoute>
        } />
        <Route path="/employer/profile" element={
          <ProtectedRoute role="NhaTuyenDung"><CompanyProfile /></ProtectedRoute>
        } />
        <Route path="/employer/service-packages" element={
          <ProtectedRoute role="NhaTuyenDung"><PackagePlans /></ProtectedRoute>
        } />
        <Route path="/admin" element={
          <ProtectedRoute role="Admin"><AdminLayout /></ProtectedRoute>
        }>
          <Route path="pending-jobs" element={<ApproveJobs />} />
          <Route path="removed-jobs" element={<RemovedJobs />} />
          <Route path="accounts/employers" element={<AdminAccounts vaiTro="NhaTuyenDung" />} />
          <Route path="accounts/candidates" element={<AdminAccounts vaiTro="UngVien" />} />
          <Route path="skills" element={<AdminSkills />} />
          <Route path="industries" element={<AdminIndustries />} />
          <Route path="reports" element={<AdminReports />} />
          <Route path="packages" element={<PackageManagement />} />
        </Route>
        <Route path="/candidate/cvs" element={
          <ProtectedRoute role="UngVien"><ManageCv /></ProtectedRoute>
        } />
        <Route path="/candidate/profile" element={
          <ProtectedRoute role="UngVien"><Profile /></ProtectedRoute>
        } />
        <Route path="/candidate/applications" element={
          <ProtectedRoute role="UngVien"><MyApplications /></ProtectedRoute>
        } />
        <Route path="/candidate/favorites" element={
          <ProtectedRoute role="UngVien"><Favorites /></ProtectedRoute>
        } />
      </Routes>
      </Suspense>
      </div>
      <Footer />
    </div>
  )
}

export default App
