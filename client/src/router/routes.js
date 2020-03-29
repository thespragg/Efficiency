const Home = () => import('../Views/Home.vue');
const Login = () => import('../Views/Login');
const Register = () => import('../Views/Register')

export default [
    { name: "Home", path: '/', component: Home },
    { name: "Login", path: '/login', component: Login },
    { name: "Register", path: '/register', component: Register },
    { name: 'Default', path: '*', redirect: '/' }
]