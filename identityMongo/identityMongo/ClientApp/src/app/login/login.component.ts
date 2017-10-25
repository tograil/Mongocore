import { Component, OnInit } from '@angular/core';
import {FormGroup, AbstractControl, FormBuilder, Validators, FormControl} from '@angular/forms';


@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  public formLogin:FormGroup = new FormGroup({
    'username': new FormControl(),
    'password': new FormControl()
  });
  public formRegister:FormGroup;
  register: boolean = false;

  constructor(private fb: FormBuilder) { }

  loginSubmit() {
    alert('login');
  }

  registerSubmit() {
    alert('register');
  }



  ngOnInit() {
  }

}
